
# Constants:
```.c
int8_t GROWL_MSG_REGISTER = 0;
int8_t GROWL_MSG_NOTIFICATION = 1;
uint16_t FLAG_UNKN_0 = 0b0000000000000001;
uint16_t MASK_PRIOLV = 0b0000000000001110;
uint16_t FLAG_NGPRIO = 0b0000000000010000;
uint16_t FLAG_UNKN_1 = 0b0000000000100000;
uint16_t FLAG_UNKN_2 = 0b0000000001000000;
uint16_t FLAG_UNKN_3 = 0b0000000010000000;
uint16_t FLAG_STICKY = 0b0000000100000000;
```

# Common structs:
```.c
typedef struct {
   uint16_t length;
   char* data[];
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
int8_t version; // 1
int8_t messageType;
```

## Message:
Either `Notification message` or `Register message` struct

## Checksum:
md5 hash of header + message + password as ascii hex string, high nibble first
```.c
char[32] checksumHash;
```

## Register message structure:
```.c
Header
Data
```

### Header:

```.c
uint16_t name_length;
int8_t notifications_length;
int8_t defaults_length;
```
            
### Data:

```.c
growl_string[notifications_length] notifications;
int8_t[defaults_length] defaults;
```   

## Notification message structure:
```.c
Header
Data
```

### Header:
```.c
uint16_t flagsAndPriority;
uint16_t name_length;
uint16_t title_length;
uint16_t message_length;
uint16_t appname_length;
```
            
### Data:
```.c
char name[name_length];
char title[title_length];
char message[message_length];
char appname[appname_length];
```   
