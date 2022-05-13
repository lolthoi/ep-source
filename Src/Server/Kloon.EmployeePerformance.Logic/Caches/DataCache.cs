using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Caches
{
    #region Base cache

    public abstract class DataCache<TKey, TData> where TData : class
    {
        #region Variables

        private Dictionary<TKey, TData> _dataSources = new Dictionary<TKey, TData>();

        internal protected Func<TKey, TKey> FixKeyFunc { get; private set; }

        private DateTime _nullAppClearTime;

        private readonly int _cleanSeconds;

        internal protected readonly object _lockObj = new object();

        #endregion

        #region Constructors

        protected DataCache(int cleanSeconds)
        {
            if (cleanSeconds < 3)
                cleanSeconds = 3;

            _cleanSeconds = cleanSeconds;

            RefreshCleanTime();
        }

        protected DataCache(Func<TKey, TKey> fixKeyFunc, int cleanSeconds)
        {
            FixKeyFunc = fixKeyFunc;

            if (cleanSeconds < 3)
                cleanSeconds = 3;

            _cleanSeconds = cleanSeconds;

            RefreshCleanTime();
        }

        #endregion

        #region Private and internal functions

        private void FixKey(ref TKey key)
        {
            if (FixKeyFunc != null)
                key = FixKeyFunc(key);
        }

        internal protected void RefreshCleanTime()
        {
            _nullAppClearTime = DateTime.Now.AddSeconds(_cleanSeconds);
        }

        internal protected bool NeedToRefresh()
        {
            return _nullAppClearTime < DateTime.Now;
        }

        internal protected virtual bool TryGet(TKey key, out TData data)
        {
            FixKey(ref key);

            if (_dataSources.TryGetValue(key, out data))
            {
                return true;
            }

            return false;
        }

        internal protected bool TrySet(TKey key, TData dataItem)
        {
            FixKey(ref key);

            try
            {
                _dataSources[key] = dataItem;
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal protected void SetDataSource(Dictionary<TKey, TData> dataSources)
        {
            if (dataSources == null)
                return;
            _dataSources = dataSources;
        }

        internal protected ICollection<TData> GetDataSource()
        {
            return _dataSources.Values;
        }

        #endregion

        public Guid Version { get; protected set; } = Guid.NewGuid();

        public abstract TData Get(TKey key);

        public abstract TData Get(TKey key, TData defaultData);


        public virtual bool ContainsKey(TKey key)
        {
            FixKey(ref key);

            return _dataSources.ContainsKey(key);
        }

        public virtual bool Remove(TKey key)
        {
            var removed = _dataSources.Remove(key);
            return removed;
        }

        public virtual void Clear()
        {
            _dataSources = new Dictionary<TKey, TData>();

            Version = Guid.NewGuid();
        }

    }

    #endregion

    #region Data All cache

    public class DataAllCache<TKey, TData> : DataCache<TKey, TData> where TData : class
    {
        public delegate Dictionary<TKey, TData> NeedResource(object sender, params object[] paramArrs);

        private bool? _isInit;
        public bool IsInitData
        {
            get { return _isInit == true; }
        }

        public DataAllCache(int cleanSeconds = 3) : base(cleanSeconds)
        {
        }

        public DataAllCache(Func<TKey, TKey> fixKeyFunc, int cleanSeconds = 3) : base(fixKeyFunc, cleanSeconds)
        {
        }

        public event NeedResource OnNeedResource;

        private void InitResource()
        {
            if (_isInit == true || (_isInit.HasValue && !NeedToRefresh()))
                return;

            lock (_lockObj)
            {
                if (_isInit == true || (_isInit.HasValue && !NeedToRefresh()))
                    return;

                try
                {
                    var dataSources = OnNeedResource(this);

                    if (dataSources != null)
                    {
                        _isInit = true;

                        SetDataSource(dataSources);
                        Version = Guid.NewGuid();
                    }
                    else
                    {
                        _isInit = false;

                        RefreshCleanTime();
                    }
                }
                catch (Exception ex)
                {
                    _isInit = null;
                }
            }
        }

        public override TData Get(TKey key)
        {
            InitResource();

            if (base.TryGet(key, out TData data))
            {
                return data;
            }
            return default(TData);
        }

        public override TData Get(TKey key, TData defaultValue)
        {
            InitResource();

            if (base.TryGet(key, out TData data))
            {
                return data;
            }
            return defaultValue;
        }

        public ICollection<TData> GetValues()
        {
            InitResource();

            return base.GetDataSource();
        }

        public override bool ContainsKey(TKey key)
        {
            InitResource();

            return base.ContainsKey(key);
        }

        public override void Clear()
        {
            _isInit = null;

            base.Clear();
        }

        public override bool Remove(TKey key)
        {
            InitResource();

            return base.Remove(key);
        }

    }

    #endregion

    #region Data Key Cache

    public class DataKeyCache<TKey, TData> : DataCache<TKey, TData> where TData : class
    {
        public delegate TData NeedValueIfKeyNotFound(object sender, params object[] paramArrs);

        public event NeedValueIfKeyNotFound OnNeedValueIfKeyNotFound;

        public DataKeyCache(int cleanSeconds = 3) : base(cleanSeconds)
        {
        }

        public DataKeyCache(Func<TKey, TKey> fixKeyFunc, int cleanSeconds = 3) : base(fixKeyFunc, cleanSeconds)
        {
        }

        private TData TryToSetAndGetData(TKey key, TData defaultData)
        {
            lock (_lockObj)
            {
                if (base.TryGet(key, out TData data))
                {
                    return data;
                }
                try
                {
                    var value = OnNeedValueIfKeyNotFound(this, key);
                    if (value == null)
                    {
                        RefreshCleanTime();
                    }

                    return TrySet(key, value) ? value : defaultData;
                }
                catch
                {
                    return defaultData;
                }
            }
        }

        public override TData Get(TKey key)
        {
            if (base.TryGet(key, out TData data))
            {
                if (data == null && NeedToRefresh())
                {
                    return TryToSetAndGetData(key, default(TData));
                }
                return data;
            }

            return TryToSetAndGetData(key, default(TData));
        }

        public override TData Get(TKey key, TData defaultData)
        {
            if (base.TryGet(key, out TData data))
            {
                if (data == null && NeedToRefresh())
                {
                    return TryToSetAndGetData(key, defaultData);
                }
                return data;
            }

            return TryToSetAndGetData(key, defaultData);
        }
    }

    #endregion
}
