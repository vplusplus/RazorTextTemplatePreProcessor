using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samples.SampleTemplates;

namespace Samples
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RenderSample01()
        {
            var template = new Sample01();
            var output = template.Render();
            Console.WriteLine(output);
        }
    }
}
