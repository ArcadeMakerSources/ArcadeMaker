using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exp
{
    public partial class Interpreter
    {
        internal HashSet<ExpError> Errors { get; } = [];
        internal void Error(string msg, Span throwing = null, bool beforeCurrentSpan = true)
        {
            GetLocLine(beforeCurrentSpan, out int line, out int col, out string sourceName, throwing);
            var err = new ExpError(sourceName, line, col, msg);
            Errors.Add(err);
            //throw err;
        }

        private void GetLocLine(bool beforeCurrentSpan, out int line, out int col, out string sourceName, Span throwing = null)
        {
            //string source = this.source;
            //sourceName = "UnknownSource";
            //int loc = 0;
            //if (SourceSpans.Length > 0 && spansCursor > 0)
            //{
            //    loc = SourceSpans[spansCursor - 1].location;
            //    source = SourceSpans[spansCursor - 1].doc?.Script ?? source;
            //    sourceName = SourceSpans[spansCursor - 1].doc?.Name ?? sourceName;
            //}

            //if (beforeCurrentSpan && lastSpan != null)
            //    loc -= lastSpan.FullText.Length;
            bool throwingAttached = throwing != null;
            throwing ??= lastSpan;
            if (!throwingAttached)
            {
                if (RunOpsRunning)
                {
                    if (!OperationsSpanPair.TryGetValue(lastOp, out throwing))
                        throw new Exception("An operation without span pair found.");
                }
            }

            string source = this.source;
            sourceName = "UnknownSource";
            int loc = 0;
            if (throwing != null)
            {
                loc = throwing.DocumentLocation;
                source = throwing.Document?.Script ?? source;
                sourceName = throwing.Document?.Name ?? sourceName;
            }

            if (!throwingAttached && beforeCurrentSpan && lastSpan != null)
                loc -= lastSpan.FullText.Length;

            line = 1;
            col = 0;
            for (int i = 0; i < loc; i++)
            {
                col++;
                if (i >= source.Length)
                {
                    line = -1;
                    col = 0;
                    break;
                }
                if (source[i] == '\n')
                {
                    line++;
                    col = 0;
                }
            }
        }

        [DoesNotReturn]
        public T ThrowRuntime<T>(string msg, string type, Span? throwing = null, bool beforeCurrentSpan = true)
        {
            ThrowRuntime(msg, type, throwing, beforeCurrentSpan);
            throw new Exception(type + ": " + msg);
        }

        [DoesNotReturn]
        public void ThrowRuntime(string msg, string type, Span? throwing = null, bool beforeCurrentSpan = true)
        {
            //if (!RunOpsRunning)
            //    return;
            var ex = new Instance(ClassDefSpan.ExpExceptionDef ?? throw new Exception("Exp system::Exception class was not defined."));
            ex.Vars[0].SetSkippingConstant(msg.ToExpString());
            ex.Vars[1].SetSkippingConstant(type.ToExpString());
            ThrowRuntime(ex, throwing, beforeCurrentSpan);
            throw null;
        }

        internal void ThrowRuntime(Instance ex, Span? throwing = null, bool beforeCurrentSpan = true, bool byExpThrowStmt = false)
        {
            if (ex == null || ex.def != ClassDefSpan.ExpExceptionDef)
                ThrowRuntime(nameof(ex) + $" was not an instance of {((IDefination)ClassDefSpan.ExpExceptionDef).FullName}.", RuntimeException.INVALID_ARGUMENT, throwing);

            GetLocLine(beforeCurrentSpan, out int line, out int col, out string sourceName, throwing);

            ex.Vars[2].SetSkippingConstant(StringToExpString(sourceName));
            ex.Vars[3].SetSkippingConstant(((double)line).ToExp());
            ex.Vars[4].SetSkippingConstant(((double)col).ToExp());
            throw new RuntimeException(ex, ex.Vars[0].Value?.ToString(), ex.Vars[1].Value?.ToString(), sourceName, line, col, byExpThrowStmt: byExpThrowStmt);
        }
    }
}