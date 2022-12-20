namespace XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;
public class ParallelTestFixture
{
    public ParallelTestFixture()
    {
        var test = 0;
    }
    private int counter = 0;
    public int Call()
    {
        counter++;

        Thread.Sleep(1000);

        return counter;
    }

    public int GetCount()
    {
        return counter;
    }
}
