% decrement
% {n:in, p:out}
% proj(and(const(b, 1), -), {a->n, ab->s})
% {{n:2, p:1}, {n:4, p:3}}

and(A, B, AB) :- B=1, AB is A - B.
decrement(N, P) :- and(N, _, P).

% ?- decrement(5, X).
% X = 4.