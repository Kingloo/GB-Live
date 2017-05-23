using System;
using GBLive.WPF;
using NUnit.Framework;

namespace GBLive.Tests
{
    [TestFixture]
    public class SimpleWebRetrieverTests
    {
        [Test]
        public void SimpleWebRetriever_Ctor_ThrowArgNullWhenParamIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleWebRetriever(null));
        }
    }
}
