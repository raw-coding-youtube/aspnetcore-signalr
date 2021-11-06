using System.Collections.Generic;
using System.Linq;

namespace Collaboration
{
    public class State
    {
        private readonly List<int> _squares = Enumerable.Range(1, 9).ToList();

        public IEnumerable<int> Squares => _squares;

        public void Move(int from, int to)
        {
            if (from == to)
                return;

            var n = _squares[from];
            _squares.RemoveAt(from);
            if (_squares.Count <= to)
            {
                _squares.Add(n);
                return;
            }

            _squares.Insert(to, n);
        }
    }
}