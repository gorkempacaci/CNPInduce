 
 
 % LAST 
 proj(and(const(b0, []), foldl(proj(and(id, cons), {a->a, a->b, b->ab}))), {as->as, b->b})



pand2(A, B, AB) :- and2(A, )
and2(A, _, AB) :- [A|A]=AB. % lists where the head and tail are the same