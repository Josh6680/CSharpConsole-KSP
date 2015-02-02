using Mono.CSharp;

namespace CSharpConsoleKSP
{
	public static class Evaluator
	{
		private static Mono.CSharp.Evaluator instance = null;

		public static Mono.CSharp.Evaluator fetch
		{
			get
			{
				if (instance == null) {
					instance = new Mono.CSharp.Evaluator(new CompilerContext(new CompilerSettings(), new EvaluatorPrinter()));
				}
				return instance;
			}
		}
	}

	public class EvaluatorPrinter : ReportPrinter
	{
		public static System.IO.TextWriter output;

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			base.Print(msg, output, showFullPath);
		}
	}
}
