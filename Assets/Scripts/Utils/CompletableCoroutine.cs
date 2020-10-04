using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class CompletableCoroutine : IEnumerator
    {
        private IEnumerator InnerCoroutine { get; }
        public bool Completed { get; private set; }

        public object Current => InnerCoroutine.Current;

        public CompletableCoroutine(IEnumerator innerCoroutine)
        {
            InnerCoroutine = innerCoroutine;
        }

        public IEnumerator Start()
        {
            while (InnerCoroutine.MoveNext())
            {
                yield return InnerCoroutine.Current;
            }

            Completed = true;
        }

        public bool MoveNext()
        {
            return InnerCoroutine.MoveNext();
        }

        public void Reset()
        {
            InnerCoroutine.Reset();
        }
    }
}
