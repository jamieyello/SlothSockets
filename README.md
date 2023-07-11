# Under construction, do not use yet.

# SlothSockets

![image](https://github.com/jamieyello/SlothSockets/assets/10054829/1d00c028-d63c-4e5a-8dd3-9b5d958453f0)

## Wow

In this library, we use allegories to mail, packages, mailboxes, and mail-people to make things easier to understand. Some key objects are;

### SlothMail

A serialized object that will be used in a package. use `new SlothMail(obj)` to serialize an object to mail and get it ready to send.

### SlothPackage

The object that will contain all mail to be sent. This can be sent to one recipient or several.

## Serialization Attributes

SlothSockets recognizes several serialization attributes as well as a custom one.

`[SlothSerialize(mode)]`: Specify whether to serialize fields or properties on a class/struct. This overrides the mode specified when serializing. Can be combined with `|`.

`[Newtonsoft.Json.JsonIgnore]`, `[System.Text.Json.Serialization.JsonIgnore]`, `[System.NonSerialized]`: Do not serialize member.
