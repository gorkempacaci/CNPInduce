% head
% {list:in, h:out}
% proj(cons, {ab->list, a->h})
% {{list:[1,2,3], h:1}, {list:[2], h:2}}

cons(A, B, [A|B]).  % (library)
head(List, H) :- cons(H, _, List).

% ?- head([1,2,3], H).
% H = 1.