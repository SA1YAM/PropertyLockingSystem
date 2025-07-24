##Issues identified:
#Race condition in LockProperty as operation is not atomic
The code checks if a property is locked using ContainsKey, then calls TryAdd. This is a classic check-then-act race condition because another thread could add the lock between the check and add calls, allowing multiple locks on the same property. Lock acquisition should be atomic.

#Lock ownership not enforced in UnlockProperty
Any client can unlock a property regardless of whether they own the lock or not, which can lead to inconsistent states.

#OnDisconnectedAsync releases only one lock per connection
If a user has multiple locks (e.g., multiple properties), only the first found lock is released on disconnect.



##Implemented Fixes:
#Fixed Race condition Issue in LockProperty by making it atomic
Used TryAdd directly to attempt atomic lock acquisition without prior ContainsKey.

#Fixed Lock ownership not enforced in UnlockProperty
Enforce ownership on unlock: only the client who owns the lock can unlock it.

#Fixed OnDisconnectedAsync releases only one lock per connection
On disconnect, release all locks held by the disconnecting client, not just the first.

#Added comments in code too for fixes and reasoning
Add comments explaining fixes and reasoning.

