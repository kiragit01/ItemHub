namespace ItemHub.Utilities;

public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _openTime;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private bool _isOpen;
    private readonly object _lock = new object();

    public CircuitBreaker(int failureThreshold, TimeSpan openTime)
    {
        _failureThreshold = failureThreshold;
        _openTime = openTime;
        _failureCount = 0;
        _lastFailureTime = DateTime.MinValue;
        _isOpen = false;
    }

    public bool IsOpen
    {
        get
        {
            lock (_lock)
            {
                if (_isOpen && (DateTime.UtcNow - _lastFailureTime) > _openTime)
                {
                    // Сбрасываем состояние после времени ожидания
                    _isOpen = false;
                    _failureCount = 0;
                }
                return _isOpen;
            }
        }
    }

    public void RecordFailure()
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            if (_failureCount >= _failureThreshold)
            {
                _isOpen = true;
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _failureCount = 0;
            _isOpen = false;
        }
    }
}
