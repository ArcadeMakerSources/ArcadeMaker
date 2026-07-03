using ArcadeMaker.Core;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Runtime;
using ArcadeMaker.IDE.Items;
using Exp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.IDE.Debugging;

internal static class Debug
{
    internal static GameRunner<FutileGame>? GameRunner { get; private set; }
    internal static event EventHandler<HashSet<ProjectError>>? OnDebugBuild;
    internal static bool TryBuild()
    {
        HashSet<ProjectError> errors = [];
        try
        {
            //List<ScriptDocument> sources = Environment.project.items.OfType<GameScript>().Map(gs => {
            //    var doc = ScriptDocument.FromString(gs.Script, gs.name);
            //    doc.Namespace ??= ExpSrc.GameNamespace;
            //    doc.Usings.AddRange(ExpSrc.GlobalUsings);
            //    return doc;
            //});

            //// ArcadeMaker library sources
            //sources.AddRange(GameRunner.GetScripts());

            //// create an interpreter instance
            //var objs = Environment.project.items.OfType<GameObject>();
            //var defs = objs.Map(go => ObjectModel.GetClass(go.name, [..go.ExtraProperties]));
            //GameRunner.AddFuncsToInterpreter(Interpreter, null, defs, Environment.project.items.OfType<GameSound>().Map(s => s.name));

            if (Environment.isGameRunning)
            {
                return true;
            }

            var futileGame = new FutileGame();
            futileGame.Objects.AddRange(Environment.project.items.OfType<GameObject>().Select(go => new ObjectModel(go.name, null,
                //new(
                //go.GetEventScripts(ObjectEvent.Create)?.Scripts.Select((script, i) => ExpSrc.CreateInstanceScriptDocument($"{go.name}.Events.Create.{i + 1}", null!, script.Script)).ToArray(),
                //go.GetEventScripts(ObjectEvent.Step)?.Scripts.Select((script, i) => ExpSrc.CreateInstanceScriptDocument($"{go.name}.Events.Step.{i + 1}", null!, script.Script)).ToArray(),
                //go.GetEventScripts(ObjectEvent.Draw)?.Scripts.Select((script, i) => ExpSrc.CreateInstanceScriptDocument($"{go.name}.Events.Draw.{i + 1}", null!, script.Script, ExpSrc.CURRENT_VIEW_INDEX_ARG_NAME)).ToArray()
                //)
                [.. go.Events.Map(e => { e.Docs?.Clear(); return e; })] // clear the documents that were created on prev debug build
                , [.. go.ExtraProperties])));

            // (now done at ObjectModel ctor):
            //futileGame.Objects.ForEach(model =>
            //{
            //    model.Events.Create?.ForEach(script => script?.Def = model.Class);
            //    model.Events.Step?.ForEach(script => script?.Def = model.Class);
            //    model.Events.Draw?.ForEach(script => script?.Def = model.Class);
            //});
            futileGame.Sprites.AddRange(Environment.project.items.OfType<GameSprite>().Map(s => new Core.Resources.Sprite(s.name, null, 0, 0, 0, null)));
            futileGame.FontsData.AddRange(Environment.project.items.OfType<GameFont>().Map(r => new Core.Resources.Serializeables.GameFont() { Name = r.name }));
            futileGame.Sounds.AddRange(Environment.project.items.OfType<GameSound>().Map(s => new Core.Resources.Sound(s.name, "", 0, 0, 0, Core.Resources.Sound.Types.SoundEffect)));
            futileGame.Scripts.AddRange(Environment.project.items.OfType<GameScript>().Map(script => ScriptDocument.FromString(script.Script, script.name)));
            futileGame.Paths.AddRange(Environment.project.items.OfType<GamePath>().Map(p => new Core.Resources.Path(p.name, 0, 0, [])));
            futileGame.Rooms.AddRange(Environment.project.items.OfType<GameRoom>().Map(r => new RoomModel(r.name, "", 0, 0, default, new([]))));

            GameRunner = new(futileGame, removeEmptyEvents: false);

            // catch errors in event scripts
            futileGame.Objects.ForEach(model =>
            {
                model.Events.ForEach(e => e.Docs?.ForEach(script => { ExpError[]? errors = null; script?.TryPrepare(GameRunner.Interpreter, out errors); CatchErrors(errors.Map(e => new ProjectError(e))); }));
            });

            //Interpreter.Build(ScriptDocument.FromString("", "main.script"), defs, [..sources]);

            // objects classes & event scripts
            //int i = 0;
            //foreach (var obj in objs)
            //{
            //    ExpError[]? createErrors = null, stepErrors = null, drawErrors = null;
            //    if (obj.createEventScript != null)
            //        ExpSrc.CreateInstanceScriptDocument($"{obj.name}.CreateEvent", defs[i], obj.createEventScript.Script).TryPrepare(Interpreter, out createErrors);
            //    if (obj.stepEventScript != null)
            //        ExpSrc.CreateInstanceScriptDocument($"{obj.name}.StepEvent", defs[i], obj.stepEventScript.Script).TryPrepare(Interpreter, out stepErrors);
            //    if (obj.drawEventScript != null)
            //        ExpSrc.CreateInstanceScriptDocument($"{obj.name}.DrawEvent", defs[i], obj.drawEventScript.Script).TryPrepare(Interpreter, out drawErrors);

            //    CatchErrors(createErrors);
            //    CatchErrors(stepErrors);
            //    CatchErrors(drawErrors);
            //}

            // validate object events:
            foreach (var obj in Environment.project.items.OfType<GameObject>())
            {
                // collision events
                foreach (var colEv in obj.Events.OfType<CollisionEvent>())
                {
                    if (Environment.project.GetItem<GameObject>(colEv.Param) == null)
                    {
                        Solutions.RemoveCollisionWithDeletedObjectsSolution allSolution = new(colEv.Param);
                        Solutions.RemoveCollisionWithDeletedObjectsSolution specificSolution = new(colEv.Param, obj);
                        CatchErrors(new ProjectError(ProjectError.Source_Engine, $"Object '{obj.name}' subscribes a collision event with an object that does not exist anymore ('{colEv.Param}').", obj.name + ".Events", 0, allSolution, specificSolution));
                    }
                }
            }
        }
        catch (BuildFailureException ex)
        {
            CatchErrors(ex.Errors.Map(e => new ProjectError(e)));
        }
        catch (Exception ex) when (false)
        {
            CatchErrors([new("Debugger", $"Uncaught build exception: {ex.Message}.", "", 0, [])]);
        }

        void CatchErrors(params IEnumerable<ProjectError> ex)
        {
            errors.AddRange(ex ?? []);
        }

        OnDebugBuild?.Invoke(Environment.project, errors);

        return errors.Count == 0;
    }

    internal static void FillErrors(this ListView errorsBox, IEnumerable<ProjectError> errors)
    {
        errorsBox.InvokeIfRequired(errorsBox.Items.Clear);

        foreach (var err in errors)
        {
            ListViewItem errItem = new(err.In) { Tag = err };
            errItem.SubItems.Add(err.Message);
            errItem.SubItems.Add(err.File);
            errItem.SubItems.Add(err.Line >= 1 ? err.Line.ToString() : "");
            errorsBox.InvokeIfRequired(() => errorsBox.Items.Add(errItem));
        }
    }

    private static ContextMenuStrip? errorsMenu;
    internal static void AttachMenu(this ListView errorsBox)
    {
        errorsBox.MouseClick += (s, e) =>
        {
            if (e.Button != MouseButtons.Right || errorsBox.SelectedItems.Count == 0)
                return;

            // get the error instance
            ProjectError error = (ProjectError)errorsBox.SelectedItems[0].Tag!;

            // create solve menu
            ToolStripMenuItem solveMenu = new("Solve");
            foreach (var solution in error.Solutions)
            {
                ToolStripMenuItem solveBtn = new(solution.ButtonText);
                solveBtn.Click += (ss, ee) => solution.Apply();
                solveMenu.DropDownItems.Add(solveBtn);
            }
            solveMenu.Enabled = solveMenu.DropDownItems.Count >= 1;

            // create copy button
            ToolStripMenuItem copyBtn = new("Copy");
            copyBtn.Click += (s, e) => Clipboard.SetText(error.Message);

            // create the main menu
            errorsMenu?.Dispose();
            errorsMenu = new();
            errorsMenu.Items.AddRange([solveMenu, copyBtn]);

            // show main menu
            errorsMenu.Show(errorsBox, e.Location);
        };
    }

    internal static void InvokeIfRequired(this Control control, Action action)
    {
        if (control.InvokeRequired)
            control.Invoke(action);
        else
            action();
    }
}