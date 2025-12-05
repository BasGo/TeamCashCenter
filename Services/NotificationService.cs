namespace TeamCashCenter.Services
{
    public interface INotificationService
    {
        
    }
    public class NotificationService
    {
        private readonly List<Notification> _notifications = [];

        public IReadOnlyList<Notification> Notifications => _notifications.AsReadOnly();

        public event Action? Changed;

        public void Set(Notification n)
        {
            // alias for Add
            Add(n);
        }

        private void Add(Notification n)
        {
            _notifications.Add(n);
            Changed?.Invoke();
        }
        
        public void ShowSuccess(string message)
        {
            _notifications.Add(new  Notification { Message = message, Type = "success" });
            Changed?.Invoke();
        }
        
        public void ShowInfo(string message)
        {
            _notifications.Add(new  Notification { Message = message, Type = "info" });
            Changed?.Invoke();
        }
        
        public void ShowError(string message)
        {
            _notifications.Add(new  Notification { Message = message, Type = "error" });
            Changed?.Invoke();
        }

        public void Remove(Notification n)
        {
            if (_notifications.Remove(n))
                Changed?.Invoke();
        }

        public void ClearAll()
        {
            _notifications.Clear();
            Changed?.Invoke();
        }

        // Backwards-compatible API
        public Notification? Current => _notifications.FirstOrDefault();

        public void Clear()
        {
            if (_notifications.Count > 0)
            {
                _notifications.RemoveAt(0);
                Changed?.Invoke();
            }
        }
    }

    public class Notification
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "success"; // success | error | info
    }
}
