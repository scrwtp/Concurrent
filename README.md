Concurrent
======

Promises and Futures implemented in terms of MailboxProcessor.

I've watched Douglas Crockford's talk [Monads and Gonads](http://www.youtube.com/watch?v=b0EF0VTs9Dc) recently, and decided to take a stab at porting promises as defined there into F#. I ended up scrapping it halfway and drafted another, more Actor-based implementation around MailboxProcessor. 

Seems nicer. 
