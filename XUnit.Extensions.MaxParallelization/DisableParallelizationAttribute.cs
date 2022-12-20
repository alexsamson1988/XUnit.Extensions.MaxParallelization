namespace XUnit.Extensions.MaxParallelization;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class DisableParallelizationAttribute : Attribute
{
}