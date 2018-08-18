using System;
using System.Collections.Generic;

namespace Piksel.GrowlLib.Server
{
    public class Headers: Dictionary<string, string>
    {
        public int GetInt(string key, int defaultValue = 0)
            => (ContainsKey(key) && int.TryParse(this[key], out int value)) ? value : defaultValue;

        public string GetString(string key, string defaultValue = "")
            => ContainsKey(key) ? this[key] : defaultValue;

        public IEnumerable<string> GetStrings()
        {
            foreach (var kv in this)
            {
                yield return $"{kv.Key}: {kv.Value}";
            }
            foreach (var group in Groups)
            {
                foreach (var str in group.Value.GetStrings())
                {
                    yield return $"{group.Key}-{str}";
                }
            }
        }

        public void Add(string key, string value, ArraySegment<string> groups)
        {
            var next = groups.Offset + 1;
            if (groups.Count == 0)
            {
                if (ContainsKey(key))
                {
                    Console.WriteLine($"Headers already contains key \"{key}\"");
                    return;
                }
                Add(key, value);

            }
            else
            {
                var groupName = groups.Array[groups.Offset];
                if (!Groups.ContainsKey(groupName))
                {
                    Groups.Add(groupName, new Headers());
                }
                Groups[groupName].Add(key, value, new ArraySegment<string>(groups.Array, next, groups.Count - 1));
            }
        }

        public Dictionary<string, Headers> Groups { get; set; } 
            = new Dictionary<string, Headers>();
    }

    public class RequestHeaders : Headers
    {
        public RequestHeaders()
        {
            Groups.Add("Application", new NamedHeaders());
            Groups.Add("Origin", new OriginHeaders());
            Groups.Add("Notifications", new NotificationsHeaders());
            Groups.Add("Notification", new NotificationHeaders());
        }

        public NamedHeaders Application => Groups["Application"] as NamedHeaders;
        public OriginHeaders Origin => Groups["Origin"] as OriginHeaders;
        public NotificationsHeaders Notifications => Groups["Notifications"] as NotificationsHeaders;
    }

    public class ResponseHeaders : Headers
    {
        public ResponseHeaders()
        {
            Groups.Add("Error", new ErrorHeaders());
        }

        public ErrorHeaders Error => Groups["Error"] as ErrorHeaders;
    }

    public class ErrorHeaders : Headers
    {
        public int Code
        {
            get => GetInt("ID");//, out string value) ? value : null;
            set => this["Code"] = value.ToString();
        }

        public string Description
        {
            get => TryGetValue("Title", out string value) ? value : null;
            set => this["Title"] = value;
        }
    }

    public class NamedHeaders : Headers
    {
        public string Name
        {
            get => GetString("Name");
            set => this["Name"] = value;
        }
    }

    public class IdentityHeaders : NamedHeaders
    {
        public string Version
        {
            get => GetString("Version");
            set => this["Version"] = value;
        }
    }

    public class OriginHeaders : Headers
    {
        public OriginHeaders()
        {
            Groups.Add("Machine", new NamedHeaders());
            Groups.Add("Software", new IdentityHeaders());
            Groups.Add("Platform", new IdentityHeaders());
        }

        public NamedHeaders Machine => Groups["Machine"] as NamedHeaders;
        public IdentityHeaders Software => Groups["Software"] as IdentityHeaders;
        public IdentityHeaders Platform => Groups["Platform"] as IdentityHeaders;
    }

    public class NotificationsHeaders : Headers
    {
        public new int Count
        {
            get => GetInt("Count");
            set => this["Count"] = value.ToString();
        }
    }

    public class NotificationHeaders : NamedHeaders
    {
        public string ID => TryGetValue("ID", out string value) ? value : null;
        public string Title => TryGetValue("Title", out string value) ? value : null;
        public string Text => TryGetValue("Text", out string value) ? value : null;
        public string Sticky => TryGetValue("Sticky", out string value) ? value : null;
        public string Priority => TryGetValue("Priority", out string value) ? value : null;
    }

}