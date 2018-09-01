
# Constants:
```.c
sbyte GROWL_MSG_REGISTER = 0;
sbyte GROWL_MSG_NOTIFICATION = 1;
ushort FLAG_UNKN_0 = 0b0000000000000001;
ushort MASK_PRIOLV = 0b0000000000001110;
ushort FLAG_NGPRIO = 0b0000000000010000;
ushort FLAG_UNKN_1 = 0b0000000000100000;
ushort FLAG_UNKN_2 = 0b0000000001000000;
ushort FLAG_UNKN_3 = 0b0000000010000000;
ushort FLAG_STICKY = 0b0000000100000000;
```

# Common structs:
```
typedef struct {
   sbyte length;
   byte* data[];
} growl_string;
```

# Packet structure:
```.c
Header
Message
Checksum
```

## Header:

```.c
sbyte version; // 1
sbyte messageType;
```

## Message:
Either `Notification message` or `Register message` struct

## Checksum:
md5 hash of header + message + password as ascii hex string, high nibble first
```.c
byte[32] checksumHash;
```

## Register message structure:
```.c
Header
Data
```

### Header:

```.c
ushort flags;
ushort name_length;
sbyte notifications_length;
sbyte defaults_length;
```
            
### Data:

```.c
growl_string[notifications_length] notifications;
sbyte[defaults_length] defaults;
```   

## Notification message structure:
```.c
Header
Data
```

### Header:
```.c
ushort flagsAndPriority;
ushort name_length;
ushort title_length;
ushort message_length;
ushort appname_length;
```
            
### Data:
```.c
byte[name_length] name;
byte[title_length] title;
byte[message_length] message;
byte[appname_length] appname;
```   
