using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DataAttributeShouldBeUsedOnATheoryFixer : BatchedCodeFixProvider
{
	const string MarkAsTheoryTitle = "Mark as Theory";
	const string removeDataAttributesTitle = "Remove Data Attributes";

	public DataAttributeShouldBeUsedOnATheoryFixer() :
		base(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				MarkAsTheoryTitle,
				ct => MarkAsTheoryAsync(context.Document, methodDeclaration, ct),
				equivalenceKey: MarkAsTheoryTitle
			),
			context.Diagnostics
		);

		context.RegisterCodeFix(
			new RemoveAttributesOfTypeCodeAction(removeDataAttributesTitle, context.Document, methodDeclaration.AttributeLists, Constants.Types.XunitSdkDataAttribute),
			context.Diagnostics
		);
	}

	async Task<Document> MarkAsTheoryAsync(
		Document document,
		MethodDeclarationSyntax methodDeclaration,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.ReplaceNode(
			methodDeclaration,
			(node, generator) => generator.InsertAttributes(node, 0, generator.Attribute(Constants.Types.XunitTheoryAttribute))
		);

		return editor.GetChangedDocument();
	}
}