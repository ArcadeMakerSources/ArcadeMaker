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
    internal static GameRunner? GameRunner { get; private set; }
    internal static bool TryBuild(out HashSet<ExpError> errors)
    {
        HashSet<ExpError> _errors = [];
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
                errors = [];
                return true;
            }

            var futileGame = new FutileGame();
            futileGame.Objects.AddRange(Environment.project.items.OfType<GameObject>().Select(go => new ObjectModel(go.name, null, new(
                go.GetEventScripts(ObjectEvent.Create)?.Scripts.Select((script, i) => ExpSrc.CreateInstanceScriptDocument($"{go.name}.Events.Create.{i + 1}", null!, script.Script)).ToArray(),
                go.GetEventScripts(ObjectEvent.Step)?.Scripts.Select((script, i) => ExpSrc.CreateInstanceScriptDocument($"{go.name}.Events.Step.{i + 1}", null!, script.Script)).ToArray(),
                go.GetEventScripts(ObjectEvent.Draw)?.Scripts.Select((script, i) => ExpSrc.CreateInstanceScriptDocument($"{go.name}.Events.Draw.{i + 1}", null!, script.Script, ExpSrc.CURRENT_VIEW_INDEX_ARG_NAME)).ToArray()
            ), [.. go.ExtraProperties])));
            futileGame.Objects.ForEach(model =>
            {
                model.EventScripts.Create?.ForEach(script => script?.Def = model.Class);
                model.EventScripts.Step?.ForEach(script => script?.Def = model.Class);
                model.EventScripts.Draw?.ForEach(script => script?.Def = model.Class);
            });
            futileGame.Sprites.AddRange(Environment.project.items.OfType<GameSprite>().Map(s => new Core.Resources.Sprite(s.name, null, 0, 0, 0, null)));
            futileGame.FontsData.AddRange(Environment.project.items.OfType<GameFont>().Map(r => new Core.Resources.Serializeables.GameFont() { Name = r.name }));
            futileGame.Sounds.AddRange(Environment.project.items.OfType<GameSound>().Map(s => new Core.Resources.Sound(s.name, "", 0, 0, 0, Core.Resources.Sound.Types.SoundEffect)));
            futileGame.Scripts.AddRange(Environment.project.items.OfType<GameScript>().Map(script => ScriptDocument.FromString(script.Script, script.name)));
            futileGame.Paths.AddRange(Environment.project.items.OfType<GamePath>().Map(p => new Core.Resources.Path(p.name, 0, 0, [])));
            futileGame.Rooms.AddRange(Environment.project.items.OfType<GameRoom>().Map(r => new RoomModel(r.name, "", 0, 0, default, new([]))));

            GameRunner = new(futileGame);

            // catch errors in event scripts
            ExpError[]? createErrors = null, stepErrors = null, drawErrors = null;
            futileGame.Objects.ForEach(model =>
            {
                model.EventScripts.Create?.ForEach(script => { script?.TryPrepare(GameRunner.Interpreter, out createErrors); CatchErrors(createErrors); });
                model.EventScripts.Step?.ForEach(script => { script?.TryPrepare(GameRunner.Interpreter, out stepErrors); CatchErrors(stepErrors); });
                model.EventScripts.Draw?.ForEach(script => { script?.TryPrepare(GameRunner.Interpreter, out drawErrors); CatchErrors(drawErrors); });
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
        }
        catch (BuildFailureException ex)
        {
            CatchErrors(ex.Errors);
        }
        catch (Exception ex) when (false)
        {
            CatchErrors([new("Debugger", 0, 0, $"Uncaught build exception: {ex.Message}.")]);
        }

        void CatchErrors(IEnumerable<ExpError>? ex)
        {
            _errors.AddRange(ex ?? []);
        }

        errors = _errors;
        return errors.Count == 0;
    }
}