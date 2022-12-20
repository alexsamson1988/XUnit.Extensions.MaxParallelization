using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnit.Extensions.MaxParallelization.Tests.Fixtures;
public class OrderingFixture
{
    private int counter = 0;

    public void Add(int number)
    {
        counter += number;
    }

    public int Count => counter;

}
