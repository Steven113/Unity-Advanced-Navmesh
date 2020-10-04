using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.General
{
    public interface IStateListener<TState>
    {
        void OnInitialState(TState state);
        void OnChangeState(TState state);
    }

    public interface IStateProvider<TState> {
        IDisposable Subscribe(IStateListener<TState> observer);
    }

    public class Unsubscriber<T> : IDisposable
    {
        private List<IStateListener<T>> _observers;
        private IStateListener<T> _observer;

        public Unsubscriber(List<IStateListener<T>> observers, IStateListener<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }

    public class ObservableState<TState> : IStateProvider<TState> where TState : struct
    {
        private TState State { get; set; }

        public ObservableState(TState initialState)
        {
            State = initialState;
        }

        public void Set(TState state)
        {
            if (!state.Equals(State))
            {
                _observers.ForEach(observer => observer.OnChangeState(state));
            }

            State = state;
        }
        public TState Get() => State;

        private List<IStateListener<TState>> _observers = new List<IStateListener<TState>>();
        public IDisposable Subscribe(IStateListener<TState> observer)
        {
            _observers.Add(observer);
            observer.OnInitialState(State);
            return new Unsubscriber<TState>(_observers, observer);
        }
    }
}
