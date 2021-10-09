using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Algos.Source.Caches;

namespace Algos.Source.Architectural
{
    /// <summary>
    /// Implementation of 'Chain of responsibilities' pattern.
    /// There are four modes:
    /// - First: traversing chain stops after first 'Responsibility' is found and performed.
    /// - FirstNoOrder: works like 'First' but with no order. In this case before traversing whole chain will travers LRU cache and stops if finds needed 'Responsibility'. 
    /// - All: travers through the whole chain regardless of whether 'Responsibility' was found or not (so that all the responsibilities can be performed).
    /// - StopIfFail: traversing stops when first 'Responsibility' fails.
    /// </summary>
    public sealed class ChainResponsibilities
    {
        private readonly ChainMode _mode;
        private readonly List<IResponsibility> _chain;
        private readonly LRUCache<IResponsibility> _cache;

        public int NumResponsibilities => _chain.Count;
        
        public ChainResponsibilities(ChainMode mode = ChainMode.First)
        {
            _mode = mode;
            _chain = new List<IResponsibility>(5);

            if (_mode == ChainMode.FirstNoOrder)
                _cache = new LRUCache<IResponsibility>();
        }

        public void AddResponsibility(IResponsibility responsibility)
        {
            _chain.Add(responsibility);
        }

        public bool Process(params object[] list)
        {
            switch (_mode)
            {
                case ChainMode.First:
                    return _FirstProcess(list);
                case ChainMode.FirstNoOrder:
                    return _FirstNoOrderProcess(list);
                case ChainMode.All:
                    return _AllProcess(list);
                default:
                    return _StopIfFailProcess(list);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _AllProcess(params object[] list)
        {
            var hasProcessed = false;

            foreach (var responsibility in _chain)
            {
                if (!responsibility.CanProcess(list)) continue;
                
                responsibility.Process(list);
                hasProcessed = true;
            }
            
            return hasProcessed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _FirstProcess(params object[] list)
        {
            foreach (var responsibility in _chain)
            {
                if (!responsibility.CanProcess(list)) continue;
                
                responsibility.Process(list);
                return true;
            }
            
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _FirstNoOrderProcess(params object[] list)
        {
            if (_cache.Find(_FindResponsibility, list))
                return true;
            
            foreach (var responsibility in _chain)
            {
                if (!responsibility.CanProcess(list)) continue;
                
                _cache.Add(responsibility);
                responsibility.Process(list);
                return true;
            }   
            
            return false;
        }

        private bool _FindResponsibility(IResponsibility responsibility, params object[] list)
        {
            if (!responsibility.CanProcess(list)) 
                return false;
            
            responsibility.Process(list);
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _StopIfFailProcess(params object[] list)
        {
            foreach (var responsibility in _chain)
            {
                if (responsibility.CanProcess(list))
                    responsibility.Process(list);
                else
                    return false;

            }
            
            return true;
        }
    }
    
    public interface IResponsibility
    {
        bool CanProcess(params object[] list);
        void Process(params object[] list);
    }

    public enum ChainMode
    {
        All,
        First,
        FirstNoOrder,
        StopIfFail
    }
}