using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class FactMethodMustNotHaveParametersFixerTests
{
	[Fact]
	public async void RemovesParameter()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Fact]
    public void [|TestMethod|](int x) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, FactMethodMustNotHaveParametersFixer.RemoveParametersTitle);
	}
}