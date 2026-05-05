IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

1

Generalized Fireﬂy Algorithm for Optimal Transmit
Beamforming

Tuan Anh Le and Xin-She Yang

3
2
0
2

t
c
O
7
2

]
T
I
.
s
c
[

1
v
0
6
4
8
1
.
0
1
3
2
:
v
i
X
r
a

Abstract—This paper proposes a generalized Fireﬂy Algorithm
(FA) to solve an optimization framework having objective func-
tion and constraints as multivariate functions of independent
optimization variables. Four representative examples of how
the proposed generalized FA can be adopted to solve down-
link beamforming problems are shown for a classic transmit
beamforming, cognitive beamforming, reconﬁgurable-intelligent-
surfaces-aided (RIS-aided) transmit beamforming, and RIS-aided
wireless power transfer (WPT). Complexity analyzes indicate that
in large-antenna regimes the proposed FA approaches require
less computational complexity than their corresponding interior
point methods (IPMs) do, yet demand a higher complexity than
the iterative and the successive convex approximation (SCA)
approaches do. Simulation results reveal that the proposed FA
attains the same global optimal solution as that of the IPM for
an optimization problem in cognitive beamforming. On the other
hand, the proposed FA approaches outperform the iterative, IPM
and SCA in terms of obtaining better solution for optimization
problems, respectively, for a classic transmit beamforming, RIS-
aided transmit beamforming and RIS-aided WPT.

Index Terms—Fireﬂy algorithm, nature-inspired optimization,

transmit beamforming, reconﬁgurable intelligent surfaces.

I. Introduction

Transmit beamforming problems are normally cast as

optimization problems where beamforming vectors are
optimization variables. Two fundamental optimization prob-
lems in transmit beamforming include: i) minimizing the total
transmit power subject
to signal-to-interference-plus-noise-
ratio (SINR) constraints [1]–[4]; ii) maximizing the weakest
SINR subject to a total power constraint [5], [6]. In fact, these
two problems are equivalent [7], [8]. A generalized version of
the second problem is introduced in [8] where the objective is
to maximize an arbitrary utility function of SINRs, which is
strictly increasing in every receiver’s SINR, subject to a power
constraint. The other variation of the second optimization
problem is the sum rate maximization [9], [10]. Furthermore,
additional constraints can be introduced to these fundamental
problems to capture other wireless communication applica-
tions. For instance, a soft-shaping interference constraint was
added for cognitive radio scenarios [11], [12] while a power
transfer constraint was included for simultaneous-wireless-
information-and-power-transfer scenarios [13]. In addition,
various metrics have been utilized to formulate downlink
beamforming optimization problems such as secrecy capacity

T. A. Le and X.-S. Yang are with the Faculty of Science and
t.le;

Technology, Middlesex University, London, NW4 4BT, UK. Email:
x.yang
}

This paper has been presented in part at the IEEE Vehicular Technology

@mdx.ac.uk.

{

Conference (VTC 2023-Spring), Florence, Italy, June, 20-23, 2023.

[14], energy eﬃciency [15], data transmission reliability, data
transmission security, and power transfer reliability [16].

Since the SINR is a non-convex quadratic function of
the beamforming vectors, the two fundamental beamforming
optimization problems are NP-hard and cannot be solved in
polynomial time. Fortunately, exploiting the hidden convexity
property of the SINR metric, an elegant framework was
proposed in [2] to convert these two optimization problems
into convex conic programming forms, which can be ef-
fectively solved by a standard interior point method (IPM).
Furthermore, uplink-downlink duality was utilized to derive
iterative algorithms to ﬁnd optimal beamforming vectors for
some power minimization problems, e.g., [1], [4], [17], [18].
An iterative algorithm was introduced in [9] to attain optimal
beamforming vectors for the sum rate maximization.

Numerous transmit beamforming problems can be realized
in quadratically constrained quadratic programs (QCQPs) of
beamforming vectors, which are mostly non-convex [11], [19].
To solve a QCQP problem, a semideﬁnite relaxation technique
[20] is adopted in which the original QCQP is converted to
a convex semideﬁnite programming (SDP) with new opti-
mization variables as beamforming matrices. If solving the
transformed SDP yields a rank-one optimal beamforming
matrix, then this optimal matrix is also the optimal solution
to the original QCQP. Otherwise, an approximated solution
to the original QCQP can be obtained by exploiting some
rank-one approximations or the Gaussian randomize procedure
[19]. Unfortunately, obtaining such solution requires further
computational resources yet results in a sub-optimal solution.
Optimization variables for downlink beamforming problems
may include diﬀerent types of beamforming vectors. For exam-
ple, in a reconﬁgurable-intelligent-surface-aided (RIS-aided)
communication system, see e.g., [21], [22] and references
therein, the optimization variables are active beamforming
vectors for the base station (BS) and a passive beamformimg
vector for the RIS. The objective function and/or constraints
for a RIS-aided communication system are functions of both
active and passive beamforming vectors. These beamforming
vectors are independent variables yet need to be jointly op-
timized making their problems non-convex. Widely adopted
approaches for tackling such problems are to iteratively solve
two sub-optimization problems, a.k.a., alternative optimization
(AO) approach [21], or to approximate a non-convex using
ﬁrst-order Taylor expansion, a.k.a., successive convex approx-
imation (SCA) [23]. In an AO approach, each of these two sub-
optimization problems, one variable is treated as a constant
while solving for the other. These sub-optimization problems
themselves are mostly in QCQP forms. Due to the inherent

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

2

non-convexity character of the original and sub-optimization
problems, the resulting active and passive beamforming vec-
tors may not be the global solutions. Whereas in a SCA
approach, a lower (or upper) bounded solution is normally
attained.

IPMs, a.k.a., barrier methods, are gradient based algorithms
being good at exploitation,1 a.k.a., intensiﬁcation, hence, they
are regarded as eﬀective methods to solve convex optimization
problems [25]. Unfortunately, most of transmit beamforming
problems are non-convex. Solving non-convex optimization
problems requires algorithms having better exploration2 ability
than that of the IPMs to avoid getting trapped in a local
mode. Fireﬂy algorithm (FA),
i.e., a nature-inspired algo-
rithm, possesses both exploitation and exploration abilities.
Consequently, FA is a good candidate for solving non-convex
downlink beamforming problems. FA is an easy-to-implement,
simple, and ﬂexible algorithm based on the ﬂashing char-
acters and behaviour of tropical ﬁreﬂies [24]. FA was ﬁrst
developed and published by Xin-She Yang, respectively, in
late 2007 and in 2008 [24], [26] for optimization problems
with objective and constrains being functions of a single
optimization variable. Although FA has been widely applied
to many applications [27], there has not been any signiﬁcant
work investigating the application of FA in solving transmit
beamforming problems. There were only two attempts to
adopt FA for a throughput maximization problem in [28]
and for a power minimization problem in [29]. As these two
attempts only capture two fundamental transmit beamforming
problems, it is not clear how FA can be adopted to solve other
types of transmit beamforming problems.

•

This paper takes a further step on implementing FA to solve
a wider range of transmit beamforming optimization problems.
The contributions of the paper can be summarized as follows.
The paper proposes a generalized FA to ﬁnd the optimal
solution of an optimization framework where its objective
function and constraints are multivariate functions of
multiple independent optimization variables. The prob-
lems in [28] and [29] are only two special cases of the
proposed generalized FA while the proposed generalized
FA is capable of handling a larger range of transmit
beamforming problems.
The paper shows four representative examples of how
the generalized FA can be adopted for solving transmit
beamforming problems, i.e., a classic transmit beamform-
ing approach, a cognitive beamforming approach, a RIS-
aided beamforming approach, and RIS-aided wireless
power transfer (WPT) approach. The applications of the
proposed generalized FA are beyond these four examples
which are only given to showcase how diﬀerent types of
beamforming problems can be handled by the generalized
FA.
For the sake of completeness and comparison, the iterative
closed form or SDP forms of the under investigated beam-
forming approaches are represented. The paper analyzes

•

•

1Exploitation is the ability of using any information from the problem of

interest to form new solutions which are better than the current ones [24].

2Exploration is the ability of eﬃcient exploring the search space to form

new solutions with suﬃcient diversity and far from the existing ones [24].

•

and compares the complexities of the iterative or SDP
and FA implementations of each beamforming approach.
Simulations are carried out to evaluate the performances
of the proposed FAs for the classic transmit beamforming,
cognitive beamforming, RIS-aided, and RIS-aided WPT
beamforming approaches.

·

(cid:23)

k·k

: the Euclidean norm; (

)H: the complex conjugate transpose operator; Tr (
·
0: Y is positive semideﬁnite; Ix: an x
×

Notation: Lower and upper case letter y and Y: a scalar; bold
lower case letter y: a column vector; bold upper case letter Y:
)T : the transpose operator;
a matrix;
): the trace
(
·
operator; Y
x identity
: the big O notation; CM
1
matrix;
vectors with complex elements; HM
M
×
(0, σ2): y is a zero-mean circularly
Hermitian matrices; y
symmetric complex Gaussian random variable with variance
σ2; diag (y): a diagonal matrix whose diagonal elements are
the entries of vector y; and ﬁnally diag (Y): a vector whose
entries are the diagonal elements of matrix Y.

1: the set of all M
×
M: the set of all M

∼ CN

×
×

O

II. Generalized Firefly Algorithm Framework

A. Proposed Generalized Fireﬂy Algorithm Framework

The FA was developed based on the following three ide-
alized rules [24], [26]. First, any ﬁreﬂy attracts other ﬁreﬂies
regardless of its sex. Second, the attractiveness of any ﬁreﬂy
to the other one is proportional to its brightness. Both attrac-
tiveness and brightness decrease as the distance between these
two ﬁreﬂies increases. Given two ﬂashing ﬁreﬂies, the darker
ﬁreﬂy will move towards the brighter one. If a ﬁreﬂy does
not ﬁnd any brighter one, it will make a random move. Third,
the brightness of a ﬁreﬂy depends on the landscape of the
objective function.

In this section, we propose a generalized FA to ﬁnd
optimal solution for an optimization framework containing
both objective and constraints as multivariate functions of
introduce the
independent variables. To that end, we ﬁrst
following optimization framework.

A,B,

minimize
···
subject to

,Z

f (A, B,

, Z) ,

· · ·

(1)

· · ·
· · ·

gl (A, B,
hk (A, B,

, Z)
0, l
≤
, Z) = 0, k
CMb×
where A
i.e.,
∈
Ma, Na, Mb, Nb,
1, are decision variables, a.k.a.,
optimization variables. Depending on the the values of
Ma, Na, Mb, Nb,
the decision variables can be
{
matrices, vectors, scalars, or the combination of all.

,
1, 2, . . . , L
}
1, 2, . . . K
,
}
CMz×
Nz ,

Na, B
∈
, Mz, Nz
≥

∈ {
∈ {
, Z

, Mz, Nz

CMa×

Nb ,

· · ·

· · ·

· · ·

,
}

∈

We continue by using the penalty method [24], [26] to

equivalently rewrite (1) as:

minimize
···

A,B,

,Z

where P (A, B,

· · ·

f (A, B,

· · ·

, Z) + P (A, B,

, Z) ,

· · ·

(2)

, Z) is the penalty term deﬁned as:

P (A, B,

, Z) =

· · ·

L

Xl=1

0, gl (A, B,
λlmax
{

· · ·

, Z)

2
}

K

+

Xk=1

ρk

hk (A, B,
{

· · ·

, Z)

2.
}

(3)

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

3

}

∀

In (3), λl > 0,
k, are penalty constants. Let
l, and ρk > 0,
∀
, Zi
Ai, Bi,
be the i-th ﬁreﬂy amongst the population of
{
· · ·
N ﬁreﬂies, i.e., i
. Following the second rule of
}
the FA, the brightest ﬁreﬂy is the most attractive one. Since
the proposed optimization framework is a minimization, we
deﬁne the brightness of ﬁreﬂy i as:3

1, 2,

∈ {

, N

· · ·

Ii (Ai, Bi,

, Zi) =

· · ·

f (Ai, Bi,

, Zi) + P (Ai, Bi,

1

For
I j
will move towards ﬁreﬂy j at (n + 1)-th generation as:

any
A j, B j,
(cid:16)

1, 2,
{
, Zi),

ﬁreﬂies
>

two
, Z j

∈
· · ·

· · ·

· · ·

(cid:17)

· · ·
i, j
Ii (Ai, Bi,

.

, Zi)

· · ·

(4)
, N
if
then ﬁreﬂy i

,
}

γx(r(n)

,

(cid:17)

(cid:17)

||

||

||

||

a,i j)2

· · ·

z,i j)2

(7)

(6)

b,i j)2

γz(r(n)

γy(r(n)

j −

Z(n)

j −

j −

j −

j −

, r(n)

Z(n)
i

+ α(n)

+ α(n)

+ α(n)

a,i j =

(cid:17)
B(n)
i

a,i , (5)

= Z(n)

= B(n)

= A(n)

(cid:16)
b,i j =

j −
B(n)

A(n)
i
B(n)
i

A(n)
(cid:16)
B(n)
(cid:16)

a ΛΛΛ(n)
b ΛΛΛ(n)
b,i ,

z ΛΛΛ(n)
z,i ,
, r(n)

i + βa,0e−
i + βb,0e−

i + βz,0e−
A(n)
A(n)
i

A(n+1)
i
B(n+1)
i
...
Z(n+1)
i
where r(n)
z,i j =
||
Z(n)
Z(n)
are the Cartesian distances which are not necessary
i
||
Euclidean distances yet they can be any measure eﬀectively
characterized the quantities of interest in the optimization
, βz,0 are, respectively, the attractiveness
problem; βa,0, βb,0,
at r(n)
, r(n)
a,i j = 0, r(n)
b,i j = 0,
, γz present
the variations of the attractiveness. The second terms in (5),
(6), and (7) capture the attractions. The third terms in (5),
(6), and (7) are randomizations with randomization factors
α(n)
a , α(n)
CMa×
b ,
z,i ∈
CMz×
Nz being matrices of random numbers drawn from a
Gaussian or an uniform distribution. The proposed generalized
FA for solving the optimization framework (1) is summarized
in Algorithm 1, where T is the maximum generation of the
algorithm. For any particular optimization problem subsumed
under the framework, the corresponding FA will have the same
steps as those in Algorithm 1 except the input, step 3, step 16,
step 18, step 19, and the return value.

z,i j = 0; ﬁnally γa, γb,

z and ΛΛΛ(n)

Na, ΛΛΛ(n)

CMb×

, ΛΛΛ(n)

, α(n)

a,i ∈

b,i ∈

Nb,

· · ·

· · ·

· · ·

· · ·

· · ·

B. Asymptotic Convergence and Optimality

Since the ﬁreﬂy algorithm, like quite a few other nature-
inspired algorithms, is a metaheuristic algorithm, there is no
rigorous proof of convergence so far in the current literature,
despite many applications of such metaheuristic algorithms.
In this section, we provide some intuitive discussions on the
optimality and convergence of the FA framework.4

· · ·

1) Asymptotic Optimality: Without loss of generality, let
= γz = γ, we consider two special cases of the
.

γa = γb =
variations of the attractiveness when γ
→ ∞
γ(r(n)
b,i j)2
When γ
it
→
1,
, e−
1. Therefore the attractivenesses in (5), (6),
and (7) are constant and, respectively, equal to βa,0, βb,0, and
βz,0. Equivalently, it is an idealized sky scenario where the

0 and γ
1, e−

is clear that e−

→
γ(r(n)
z,i j)2

γ(r(n)

a,i j)2

· · ·

→

→

→

0,

3Note that if (1) is a maximization problem, then (2) can be expressed as:

,Z −

f (A, B,

, Z) + P (Ai, Bi,

minimize
A,B,
···
4Mathematical analysis of the FA’s optimality and convergence deserves an
important research topic. Such analysis is postponed to future research due
to the space constraint.

, Zi).

· · ·

· · ·

Algorithm 1 Generalized Fireﬂy Algorithm for solving (1)

1: Input:

parameters:
γa, γb,

FA
βa,0, βb,0,
, βz,0,
· · ·
the structures/parameters of
gl (A, B,
· · ·
2: Randomly

, Z), hk (A, B,

· · ·

· · ·

, Z);

N,

T ,

λt,

, γz; Optimization
f (A, B,
functions

ρk,
data:
, Z),

· · ·

generate
A2, B2,
{

· · ·

N

populations

,

AN, BN,
{

· · ·

, ZN

;
}}

· · ·

A1, B1,

{{

· · ·

3: Evaluate the light intensities of N population as (4);
order
in
4: Rank

descending

the

, Z2}
,
a

, Z1}
,
ﬁreﬂies
, Zi);

best
A⋆, B⋆,
{

solution:
, Z⋆

· · ·

}

I⋆

of

:=
:=

Ii (Ai, Bi,

· · ·
the
A⋆, B⋆,
(cid:0)

5: Deﬁne
I1
A1, B1,
{

, Z⋆
· · ·
, Z1}
;
6: for n = 1 : T do
7:

for i = 1 : N do

· · ·

current
;
(cid:1)

for j = 1 : N do
if Ii (Ai, Bi,
I⋆
, Z⋆
}
end if
if I j

· · ·

:=

A⋆, B⋆,
{

Ai, Bi,
{

· · ·

, Zi

· · ·

, Zi) > I⋆ then
:=

Ii (Ai, Bi,

;
}
> I⋆ then
I j

A j, B j,
(cid:16)

, Zi);

· · ·

· · ·

, Z j

;

(cid:17)

· · ·

, Z j
:=
A j, B j,
{

· · ·

A j, B j,
(cid:16){
I⋆
, Z⋆
}
end if
if I j

:=

· · ·

}(cid:17)
, Z j

;
}

· · ·

, Z j

> Ii (Ai, Bi,

, Zi) then
A j, B j,
Move ﬁreﬂy i towards ﬁreﬂy j as (5)-(7);
(cid:16)
end if
Attractiveness
γb
, e−
,

varies with
r(n)
z,i j

distances

· · ·

γz

(cid:17)

;

2

2

via

Evaluate new solutions and update light inten-

· · ·

(cid:16)

(cid:17)

, e−

r(n)
b,i j
(cid:16)

(cid:17)

A⋆, B⋆,
{

γa

e−

r(n)
a,i j
(cid:16)

2

(cid:17)

sity as (4);

end for

end for
Rank
Ii (Ai, Bi,

23:

· · ·
Update
A⋆, B⋆,
I1
A1, B1,
(cid:0)
{
24: end for
25: return

, Zi);
the
, Z⋆
· · ·
, Z1}
;
· · ·
A⋆, B⋆,
{

· · ·

, Z⋆

.
}

the ﬁreﬂies

in

a

descending order of

current
;
(cid:1)

best
A⋆, B⋆,
{

solution:
, Z⋆

· · ·

}

I⋆

:=
:=

8:
9:

10:

11:

12:

13:

14:

15:
16:

17:

18:

19:

20:

21:
22:

brightness of each ﬁreﬂy does not change over the distance,
which can be seen everywhere. Consequently, a global opti-
mum can be obtained.

it

→

a,i j)2

b,i j)2

γ(r(n)

0, e−

,
→ ∞
γ(r(n)
z,i j)2
, e−

On the other hand, when γ
γ(r(n)

is obvious that
e−
0,
indicating
that the attractiveness of each ﬁreﬂy is zero. Equivalently,
each ﬁreﬂy is randomly in a heavily foggy region and cannot
be seen by the others. Each will randomly move and the
optimality is not always guaranteed. In this case, FA is
equivalent to a random search approach.

· · ·

→

→

0,

In fact, the attractiveness is in between these two extreme
0.5 deﬁnes the average
cases, i.e., 0 < γ <
. The value of γ−
distance of a herd of ﬁreﬂies being seen by its adjacent herds.
Hence, the entire population can be separated into number of
herds. This automatic division property provides FA suitable

∞

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

4

→

· · ·

· · ·

ability of handling highly nonlinear and multimodal optimiza-
tion problems. By controlling the attractiveness γa, γb,
, γz
, αz, it has been shown
and the roaming randomness αa, αb,
in previous studies that FA can outperform both Particle
Swarm Optimization (PSO), see, e.g., [30]–[33], and random
search approaches, see e.g., [24], [26].
2) Asymptotic Convergence: When γ

0, the convergence
of FA is similar to that of PSO where the convergence was
analyzed by Clerc and Kennedy in 2002 in [34]. When γ
,
→ ∞
the FA may act like a random search, though its behaviour
is similar to that of Simulated Annealing (SA) because the
FA’s solution is perturbed or modiﬁed in the similar way as
that in the SA in this limiting case. The SA was shown to
be convergent under the right-cooling conditions [35]. The
, αz, in
reduction of the roaming randomness, i.e., αa, αb,
the FA can be considered as a type of cooling schedule, and
thus it can be expected that FA can converge in this case.
Let us now investigate the case when 0 < γ <
.
∞
Given a very large number of ﬁreﬂy population N, it can
be assumed that N is much greater than the number of local
optima. The initial locations of N ﬁreﬂies should be uniformly
distributed over the whole search space. As the iterations of
Algorithm 1 progress, i.e., n increases, these initial N ﬁreﬂies
should converge into all locally brighter ones, i.e., the local
optima including the global ones, in a stochastic manner due
to the third term in (5), (6), and (7). By comparing the
brightest ﬁreﬂies amongst the locally brighter ones, i.e., the
best solutions amongst the local optima, the global optima
can be attained. Theoretically, these ﬁreﬂies will reach the
global optimal when N
1. However, it has
been reported in the related literature that the FA converges
with less than 50 to 100 generations [24], [26].

and n

→ ∞

· · ·

≫

In sections IV, V, and VI, we present how the proposed
FA can be adopted to solve optimization problems for trans-
mit beamforming designs.5 Hereafter, “min” and “s. t.” are,
respectively, used to represent “minimize” and “subject to”.

III. Transmit Beamforming
In this section we consider a classic transmit beamforming
problem with a well-known iterative method based on uplink-
downlink duality. We then introduce our FA solution to the
problem.

A. Problem Formulation

∈

CM

i ∈

C1
×

Mt , wi

1) Problem Formulation: Consider an Mt-antenna BS serv-
ing U single-antenna mobile users. Let hH
1
×
and si, respectively, be the channel between the i-th user
and the BS, the information-beamforming vector and the data
symbol for the ith user. The overall signal received by the ith
user is yi =
i w js j +ni where ni is a zero mean circularly
symmetric complex Gaussian noise with variance σ2, i.e.,
(0, σ2), at the user. Let Ri = hihH
ni
represent the
i
instantaneous channel state information (CSI) or Ri = E
hihH
i
wi
be the set
denote the statistical CSI,
(cid:17)
(cid:16)
{

w1, w2,
{

U
j=1 hH

∼ CN

, wU

· · ·

P

=

}

}

of candidate information-beamforming vectors for all users.
Assuming that E

= 1, the SINR at the i-th user is

2

si

|

(cid:16)|
(cid:17)
SINRi =

i Riwi
j Riw j + σ2
P
We design the set of beamforming vectors

wH
U
j=1, j,i wH

.

such that
the BS’s total transmit power is minimized while maintaining
the SINR level at each user above the required threshold. To
that end, the problem is formulated as follows:

wi
{

}

(8)

min
wi

s. t.

U

Xi=1

wH

t wt

wH
U
j=1, j,i wH

i Riwi
j Riw j + σ2

i ≥

(9)

γi,

i

∀

1,

∈ {

, U

,
}

· · ·

P

where γi is the required SINR level for the i-th user. Problem
(9) is known as non-convex due to the SINR constraint.

2) Iterative Approach: An elegant approach to solve (9)
was introduced in [1] based on uplink-downlink duality where
the optimal solution of the downlink problem can be sought
via solving the following dual-uplink problem:6

min
pi

subject to

U

pi

Xi=1
p

Γt(p),

(cid:23)
, Γ = diag
T

T

pU

i
tU (p)

,

(10)

γ1, γ2,

· · ·

, γU

,
(cid:3)

(cid:2)

where p =

p1

p2

t(p) =

t1 (p)
h

h

t2 (p)

· · ·

· · ·

,

(cid:16)P

(11)

ti (p) = arg min
ˆwi

i
ˆwH
i Qi (p) ˆwi
ˆwH
i Ri ˆwi
U
, pi = λiσ2
t=1,t,i ptRt + σ2
Qi (p) =
i I
is the dual-uplink
i
power for i-th user, λi is the ith Lagrange multiplier associated
(cid:17)
with the ith constraint in (9), and ˆwi, i.e., ˆwH
i ˆwi = 1, is the dual-
uplink beamforming vector for i-th user. Starting from any
positive initial value of p (0), the solution for the dual-uplink
problem (10) can be found iteratively as p (n + 1) = Γt (p (n)).
The iterative downlink algorithm to ﬁnd optimal solutions for
(9) is summarised in algorithm 2.

B. Proposed Fireﬂy Algorithm

We rewrite (9) as

f (W)

min
W
s. t.

di (W)

0,

i,

(12)

≤
CMt×
U
i=1 wH
where W =
, wU
i wi,
i ∈
U
j=1, j,i wH
di(W) =
i . Using the
penalty method, we recast (22) into an unconstrained problem
as:

f (W) =
j Riw j + γiσ2

w1, w2,
· · ·
i Riwi + γi

∀
U,

wH
h

P

P

−

min
W

f (W) + P(W),

(13)

5The original FA has been discretized to solve various discrete or combi-
natorial optimization problems [36]. For example, Osaba et al. [37] used a
discrete FA to solve rich vehicle routing problems.

6This approach was also adopted for transmit beamforing problems in

coordinated multi-point (CoMP) transmissions, see e.g., [38] and [39].

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

5

Algorithm 2 Iterative algorithm for problem (9)
1: Input: Γ = diag

, Ri,

, γU

γ1, γ2,
(cid:2)

· · ·

(cid:3)

i, number of

∀

iterations T .
2: Initialize p (1)
(cid:23)
3: for n = 1 : T do
4:

for i = 1 : U do

0.

Find ˆwi (n) as the dominant eigenvector of the

5:

6:

matrix Gi(n) = pi (n) Q−
i

1

(p (n)) Ri

Calculate ti (p (n)) = ˆwH

i (n)Qi(p(n)) ˆwi(n)
ˆwH
i (n)Ri ˆwi(n)

.

end for
Update p (n + 1) = Γt (p (n)).

7:
8:
9: end for
10: p⋆
11: Output: w⋆

i = p (n + 1) and ˆw⋆
i = ˆwi (n + 1).
i ˆw⋆
p⋆
i .

i =

q

where P(W) is the penalty term given as:

U

P(W) =

λimax

Xi=1

0, di(W)
{

2 ,
}

(14)

with λi > 0 is the penalty constant.

Let

Wi
{

}

=

wi

1, wi
2,

, wi
U

· · ·

Wi
initialize a population of N ﬁreﬂies
nh
io
{
and deﬁne the light density of the ﬁreﬂy

be the i-th ﬁreﬂy. We
,
}

1, 2,

, N

· · ·

, i
}
Wi
{

∈ {
as:
}

Ii (Wi) =

1
f (Wi) + P(Wi)

.

(15)

For any two ﬁreﬂies i and j
W j
I j
ﬁreﬂy j as:
(cid:17)
(cid:16)

if
> Ii (Wi) then the ﬁreﬂy i will move toward the

in the population,

r(n)
i j

γ

(cid:16)

2

(cid:17)

W(n)
(cid:16)

j −

W(n)
i

(cid:17)

+ α(n)V, (16)

W(n+1)
i

= W(n)

i + β0e−
W(n)
i

||

||

(W(n)

where r(n)
i j =
is the Cartesian distance, β0 is
j −
the attractiveness at r(n)
i j = 0, γ presents the variation of of the
attractiveness. The second term of (16) represent the attraction.
The third term of (16) is a randomization comprised of a
randomization factor α(n) and a matrix of random numbers
U. The random factor α(n) and the elements of V are
V
drawn from either a Gaussian or an uniform distribution.

CMt×

∈

It can be seen that problem (12) is a special case of the
proposed framework (1) where the objective and constraints
are functions of optimization variable W. Hence, the proposed
FA has the same steps as those in Algorithm 1 except steps
3, 16, 18 and 19 given in Algorithm 3.

Algorithm 3 Modiﬁed generalized FA for solving (12)

i , γi;

Input: FA parameters: N, T , λi, β0; Optimization data: Ri,
σ2
Step 3: Evaluate the light intensities of N ﬁreﬂies as (15);
Step 16: Move ﬁreﬂy i towards ﬁreﬂy j as (16);
Step 18: Attractiveness varies with distance via e−
(cid:17)
Step 19: Evaluate new solutions; update Ii(Wi) as (15);
return W⋆.

r(n)
i j
(cid:16)

;

γ

2

C. Complexity Analysis

The complexity of algorithm 2 is described in the following

lemma.

Lemma 1: The computational complexity of algorithm 2 is

.
i

U(M3
h

on the order of T

t + M2

t + Mt log Mt) + U

Proof: The proof is based on the observation that com-
plexities of steps 5, 6 and 8 are, respectively, on the order of
M3

t + Mt log Mt, M2
Lemma 2: The computational complexity of Algorithm 3 is

t and U.

on the order of:

T N2

h

M2

t + NU Mt(1 + U Mt)

+ T N log N + N MtU
+NU Mt(1 + U Mt) + N log N.

i

(17)

×

t , while the complexity of evaluating

Proof: Due to space limitation, we provide main obser-
vations to derive (17) as follows. The dominant terms of the
computational complexity of Algorithm 3 are at steps 2, 3,
4, 16, 19, and 22. The complexity of generating N matrices,
each matrix of size Mt
U, in step 2 is on the order of N MtU.
The complexity of evaluating each di(W) is on the order of
U M2
t wt is on the
order of U Mt.7 Hence the complexity of calculating the light
density for N ﬁreﬂies, i.e., steps 3 and 19, is on the order
of N(U Mt + U 2M2
t ) = NU Mt(1 + U Mt). The complexity of
ranking N ﬁreﬂy in steps 4 and 22 is N log N. Finally, the
complexity of moving a ﬁreﬂy in step 16 is on the order of
M2
t . Assuming a worst case when step 16 is executed in every
inner loop of the algorithm, after some manipulations, one can
arrive at (17).

U
t=1 wH

P

IV. Cognitive Beamforming

A. Problem Formulation

1) Problem Formulation: Consider a cognitive wireless
communication system consisting of an Mt-antenna cognitive
base station (BS), U active single-antenna secondary users
(SUs) and K single-antenna primary users (PUs). The cog-
nitive BS is allowed to communicate with its SUs in the
same frequency band owned by the primary system if its
interference imposed on each PU is less than a predeﬁned
tolerable threshold of Ito,k. The received signal at the t-th SU,
t

, U

1,

∈ {

· · ·

, is:
}
yt = hH

s,twt st +

U

Xj=1, j,t

hH
s,tw js j + nt,

(18)

C1
×

s,t ∈

∼ CN

Mt is the channel coeﬃcient of the wireless
where hH
1 and
link between the t-th SU and the cognitive BS; wt
(0, 1) are, respectively, the beamforming vector and
st
(0, σ2
the data symbol associated to the t-th SU; and nt
t )
is a zero mean circularly symmetric complex Gaussian noise
t , at the t-th SU. Let Rs,t = E
with variance σ2
for the
statistical CSI and Rs,t = hs,thH
(cid:16)
s,t for the instantaneous CSI.
The SINR at the t-th SU can be expressed as:

hs,thH
s,t

CMt×

∼ CN

∈

(cid:17)

SINRt =

wH
U
j=1, j,t wH

t Rs,twt
j Rs,tw j + σ2
t

.

(19)

7Here, we adopt the schoolbook iterative algorithm to evaluate complexity
p as the order

of the multiplication of two matrices of sizes n
of nmp.

m and m

×

×

P

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

p,k ∈

Let hH

C1
Mt be the channel coeﬃcient of the wireless
×
, K
link between the k-th PU, k
, and the cognitive BS,
}
Rp,k = E
for the statistical CSI and Rp,k = hp,khH
hp,khH
p,k for
p,k
the instantaneous CSI. The total interference power imposed
(cid:16)
on the k-th PU by the cognitive BS is

∈ {

· · ·

1,

(cid:17)

U
j=1 wH

j Rp,kw j.

P

Our objective is to design downlink beamforming vectors
for the SUs that minimize the cognitive BS transmit power
while maintaining the required SINR level for every SU and
keeping the interference level imposed at each PU receiver
below the predeﬁned tolerable threshold. The optimization
problem to design beamforming vectors is cast as:

min
wt

s. t.

U

Xt=1

wH

t wt

wH
U
j=1, j,t wH
P
U

t Rs,twt
j Rs,tw j + σ2

t ≥

ηt,

t

∀

1,

∈ {

, U

,
}

· · ·

(20)

wH

j Rp,kw j

Ito,k,

k

∀

1,

∈ {

, K

,
}

· · ·

≤

Xj=1

where ηt is the required SINR level for the t-th SU. Due to
the SINR constraint, problem (20) is non-convex.

2) SDP Approach: For the sake of completeness, we
provide a review on a traditional approach to solve (20)
using semideﬁnite programming (SDP). We ﬁrst form a new
optimization variable Ft = wtwH
Mt ,
is a rank-one matrix.8 We then utilize the identity
and Ft
xHXx = Tr(XxxH) to rewrite (20) as:

t where Ft

HMt×

0, Ft

(cid:23)

∈

U

M

Xt=1

Tr (Ft)

min
HM

×

Ft∈

s. t.

1 +

Tr

Rs,tFt

(cid:0)

U

−

Xj=1

(cid:1)

1
ηt !

U

Tr

Rs,tF j

(cid:16)

σ2

t ≥

0,

t,

∀

(cid:17) −

Ito,k

−

Ft

(cid:23)

Xj=1
0,

∀
, U

Tr

Rp,kF j

(cid:16)

t,

0,

k,

∀

(cid:17) ≥

(21)

where t

1,

∈ {

· · ·

, k
}

1,

∈ {

· · ·

, K

.
}

Problem (21) is in a standard SDP form. Hence, its optimal
solution can be obtained in a polynomial time by using a
general purpose IPM, e.g., CVX which is a Matlab based
modeling system for constructing and solving disciplined
convex programs [40]. In arriving at (21), we have relaxed
the rank-one constraint on Ft,
t. If the solution of (21)
does not have rank-one, then further computation resources are
required to derive a sub-optimal solution via some rank-one
approximations or the Gaussian randomize procedure [19].

∀

6

(22)

f (W)

min
W
s. t.

φt(W)
ϕk(W)

≤

0,
0,

t

∀
k

∈ {

· · ·

1,
1,

, U

, K

,
}
,
}

∀
∈ {
CMt×

≤
, wU
· · ·
j Rs,tw j + ηtσ2

· · ·
U,
f (W) =
t wt,
wH
t Rs,twt and ϕk(W) =
P
Ito,k. Using the penalty method, we ﬁrst

U
t=1 wH

i ∈

t −

where W =
φt(W) = ηt
U
j=1 wH

w1, w2,
U
j=1, j,i wH
h
j Rp,kw j
P

−

transform (22) into an unconstrained problem as:
P

min
W
where P(W) is the penalty term given as:

f (W) + P(W),

(23)

U

P(W) =

λtmax

Xt=1

0, φt(W)
{

2 +
}

K

Xk=1

ρkmax

0, ϕk(W)
{

2 ,
}

(24)

with λt > 0 and ρk > 0 are penalty constants.

wi

Let Wi =

1, wi
2,
initialize a population of N ﬁreﬂies Wi, i
∈ {
deﬁne the light density of the ﬁreﬂy Wi as:

U be the ﬁreﬂy i. We
, and
}

, wi
U

i ∈

1, 2,

, N

CMt×

· · ·

· · ·

h

Ii (Wi) =

1
f (Wi) + P(Wi)

.

(25)

For any two ﬁreﬂies i and j
W j
I j
ﬁreﬂy j as:
(cid:17)

if
> Ii (Wi) then the ﬁreﬂy i will move toward the

in the population,

(cid:16)

W(n)

j −

W(n)
i

+ α(n)V,

(26)

W(n+1)
i

= W(n)

2

(cid:17)

γ

i + β0e−
(W(n)

r(n)
i j
(cid:16)
W(n)
i

(cid:17)

||

||

(cid:16)
where r(n)
i j =
is the Cartesian distance, β0 is
j −
the attractiveness at r(n)
i j = 0, γ presents the variation of of the
attractiveness. The second term of (26) captures the attraction.
The third term of (26) is a randomization comprised of a
randomization factor α(n) and a matrix of random numbers
U. The random factor α(n) and the elements of V are
V
drawn from either a Gaussian or an uniform distribution.

CMt×

∈

It can be seen that problem (22) is a special case of the
proposed framework (1) where the objective and constraints
are functions of only one optimization variable W. Hence, the
proposed FA has the same steps as those in Algorithm 1 except
steps 3, 16, 18 and 19 given in Algorithm 4.

Algorithm 4 Modiﬁed generalized FA for solving (20)

t , ηt, Ito,k;

Input: FA parameters: N, T , λt, ρk, β0, γ; Optimization
data: Rs,t, Rp,k, σ2
Step 3: Evaluate the light intensities of N ﬁreﬂies as (25);
Step 16: Move ﬁreﬂy i towards ﬁreﬂy j as (26);
Step 18: Attractiveness varies with distance via e−
(cid:17)
Step 19: Evaluate new solutions; update Ii(Wi) as (25);
return W⋆.

r(n)
i j
(cid:16)

;

γ

2

B. Proposed Fireﬂy Algorithm

Here, we adopt the generalized FA in Algorithm 1 to solve

(20). Rearranging the constraint, we rewrite (20) as:

C. Complexity Analysis

8A matrix is rank-one if and only if it has only one linearly independent

column/row.

We investigate the complexity of solving (21) in a worst-
case runtime of the IPM followed by the complexity analysis
of the proposed FA. We start by the following deﬁnition.

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

7

Deﬁnition 1: At a given ε > 0, the set of

is an ε-
solution to problem (21), i.e., an acceptable solution with the
accuracy of ε, if

Fε
t }
{

U

Xt=1

Tr

Fε
t
(cid:0)

(cid:1)

U

min
HM
×

Ft∈

M

≤

Xt=1

Tr (Ft) + ε.

(27)

The number of decision variables of (21) is M2
plexity of (21) is described in the following lemma.

t . The com-

Lemma 3: The computational complexity to attain ε-

solution to (21) is on the order of:

1

i

(cid:16)

ln

ε−

(cid:17) p

(28)

t + 1)(U + K)
M2
t .

(M2
U(Mt + 1) + K
h
+U M2
t (M2
t + Mt) + M4
t
Proof: We sketch some main steps to arrive at the lemma
due to space limitation. It can be observed that (21) has (U+K)
linear-matrix-inequality (LMI) constraints of size 1 and U LMI
constraints of size Mt. One can follow the same steps as in
[41, Section V-A] to derive the following facts: (i) the itera-
√U(Mt + 1) + K,
ε−
tion complexity is on the order of ln
the per-iteration complexity is on the order of
and (ii)
(cid:16)
t + Mt) + M4
(M2
t
h
on the order of:

t + 1)(U + K) + U M2
Lemma 4: The computational complexity of Algorithm 4 is

(cid:17)
M2
t .

t (M2

i

1

T N2

M2

t + NU Mt(1 + U Mt + K Mt)

+ T N log N + N MtU

h

+NU Mt(1 + U Mt + K Mt) + N log N. (29)

i

×

t , while the complexity of evaluating

Proof: Due to space limitation, we provide main obser-
vations to derive (29) as follows. The dominant terms of the
computational complexity of Algorithm 4 are at steps 2, 3, 4,
16, 19, and 22. The complexity of generating N matrices, each
U, in step 2 is on the order of N MtU. The
matrix of size Mt
complexity of evaluating each φt(W) or ϕk(W) is on the order
of U M2
t wt is on
the order of U Mt. Hence the complexity of calculating the light
density for N ﬁreﬂies, i.e., steps 3 and 19, is on the order of
N(U Mt + U 2M2
t ) = NU Mt(1 + U Mt + K Mt). The
complexity of ranking N ﬁreﬂy in steps 4 and 22 is N log N.
Finally, the complexity of moving a ﬁreﬂy in step 16 is on the
order of M2
t . Assuming a worst case when step 16 is executed
in every inner loop of the algorithm, after some manipulations,
one can arrive at (29).

t + KU M2

U
t=1 wH

P

V. Reconfigurable Intelligent Surface-Aided Beamforming
A. Problem Formulation

1) Problem Formulation: Consider a communication sys-
tem comprising of an Mt-antenna BS communicating with U
single-antenna mobile users in which the direct communica-
tion links between the BS and its mobile users are blocked,
e.g., because of high building etc., [42]. To circumvent the
problem, an Nt-reﬂective-element RIS is utilized to support
Nt represent
the communication. Let H = [h1, . . . , hNt ]
the channel coeﬃcients between the BS and the RIS and
gi = [gi1, . . . , giNt ]T
1 be the channel coeﬃcients
between the RIS and the i-th user.

CMt×

CNt×

∈

∈

Let xi, i.e., E[

1, respectively,
represent the data symbol and the active beamforming vector

2] = 1, and wi

CMt ×

xi

∈

|

|

for the i-th user. Each reﬂective element of the RIS generates
a phase shift to support the communication between the BS
and the mobile users. Let θk be the phase shift at the k-th
, θNt ]T denote the
reﬂective element and let θθθ = [θ1, θ2,
phase-shift coeﬃcients generated by the RIS with
1
k = 1, . . . , Nt. Vector θθθ is the passive
and arg(θk)
beamforming vector for the RIS. The signal arrived at the i-th
user is:

π, π),

| ≤

· · ·

θk

−

∀

∈

[

|

yi = gH

i diag(θθθ)HHHwixi + gH

i diag(θθθ)HHH

U

Xj=1, j,i

w jx j + ni,

= θθθHGH

i wixi + θθθHGH
i

U

Xj=1, j,i

w jx j + ni,

(30)

CNt×

i = diag(g∗i )HH

where GH
(0, σ2)
represents the additive noise measured at the i-th user. Fur-
denote the set of active
thermore, let
}
, θθθ) be the SINR at the
beamforming vectors, and SINRi(
}
i-th user. One can write:

, wU
}
wi
{

w1, w2,
{

Mt and ni

∼ CN

wi
{

· · ·

=

∈

SINRi (

wi
{

, θθθ) =
}

U

2

.

(31)

θθθHGH
|

θθθHGH

i wi

|
i w j

2 + σ2
i

|

j=1, j,i |
P

The optimization is posed as follows:

min
, θθθ
wi}
{
s. t.

U

wH

i wi

Xi=1
, θθθ)
SINRi (
wi
}
{
k,
1,
θk

ηi,

i,

∀

≥

(32)

|

∀
where ηi is the required SINR level measured at the i-th user.
Since the SINR constraint is a function of two optimization
variables wi and θθθ, problem (32) is non-convex.

| ≤

2) Alternative Optimization Approach: For the sake of
completeness, the widely-adopted AO approach [21], [42]–
[44] is represented here as a baseline to solve (32). Let
i , and Θ = θθθθθθH, i.e., rank(Fi) = 1 and rank(Θ) = 1.
Fi = wiwH
As Fi and Θ are two independent variables, they can be
alternatively solved [21], [42]–[44]. To that end, relaxing the
rank-one constraint on Fi and beginning with any initial value
of the reﬂecting coeﬃcient matrix Θ(0), the following sub-
problem will be solved at the p-th iteration:

U

Tr

min
Fi}
{

s.

t.

Tr

Fi

Xi=1


GiΘ(p
1)GH
−
ηiσ2
i

i Fi

−

Fi

0,

i

∀

1,

∈ {

· · ·

(cid:23)

U

Xj=1, j,i
.
, U
}

Tr

GiΘ(p
1)GH
−
σ2
i

i F j

1

0,

i,

∀

≥

−

(33)

The reﬂecting coeﬃcients Θ(p) is then updated from the
, by solving
}

optimal solution of (33) at p-th iteration, i.e.,

F(p)
i
{

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

the following sub-problem [42]:

min
Θ

Tr (Θ)

It (Wt, θθθt) =

1
f (Wt) + P(Wt, θθθt)

.

8

(39)

ΘGH

j Gi

i F(p)
σ2
i

1

0,

i,

∀

≥

−

For any ﬁreﬂies t and l amongst

if
It (Wt, θθθt) > Il (Wl, θθθl) then the ﬁreﬂy l will move toward the
ﬁreﬂy t as:

the population,

s.

t.

Tr

ΘGH

i Gi

i F(p)
ηiσ2
i
diag (Θ)
(cid:1)
(cid:0)
0.

Tr

U

−

(cid:22)

Xj=1, j,i
INt ,

diag
Θ

(cid:23)

(34)
The AO approach repetitively solves two SDPs (33) and

(34) in n0 iterations to obtain the solution for (32).

Remark 1: It is worth noticing that the AO approach ap-
proximates the originally non-convex optimization (32) by
two sub-problems (33) and (34). Although (33) and (34) are
convex, the solutions to these sub-problems can be regarded
as the upper bounds of the original problem (32) as these
solutions may not be the global solution. Furthermore, the
AO approach adopts the so-called semideﬁnite relaxation
technique [20] in which the rank-one constraints on Fi and
Θ are relaxed. If solving (33) and/or (34) does not return
rank-one matrices Fi and/or Θ, then a rank-one approximation
or a Gaussian randomize procedure [19] is required to extract
approximated rank-one solutions. Extracting the approximated
solutions requires further computational resources yet only
results in sub-optimal solutions.

Motivated by the above observations, we introduce a novel
FA approach to simultaneously solve wi and θθθ for the original
problem (32) in the following section.

B. Proposed Fireﬂy Algorithm

The optimization (32) can be expressed as

f (W)

min
W, θθθ
{
}
s. t.

where W =

w1, w2,
h

· · ·

φi (W, θθθ) = ηi P

W, θθθ
φi (
{
ϕk (θk)

)
}
0,
≤
CMt×

i,

0,
k,

∀

≤
∀
U, f (W) =

, wU

i ∈
U
j GiθθθθθθHGH
j=1 wH

i w j

+ ηi

(35)

U
i=1 wH

i wi,

P

σ2
i
wH
i GiθθθθθθHGH
σ2
i
1. Adopting the penalty method, (35) can

(1 + ηi)

i wi

(36)

−

,

and ϕk (θk) =
be written as:

|

θk

| −

min
W, θθθ
}
{
where P(W, θθθ) is the penalty term given as:

f (W) + P(W, θθθ),

P(W, θθθ) =

U

Xi=1

λimax

0, φi(
{

W, θθθ
{

)
}

}

2 +

Nt

Xk=1

ρkmax

0, ϕk(θk)
{

2 , (38)
}

=

Let

Wt, θθθt
{

with λi > 0 and ρk > 0 are penalty constants.
1, wt
2,

wt
, wt
be the ﬁreﬂy t. We
U
{h
initialize a population of N ﬁreﬂies
, N
1, 2,
i
}
and deﬁne the light density, i.e., the brightness, of the ﬁreﬂy
t

, θθθt
}
Wt, θθθt
{

, t
}

∈ {

· · ·

· · ·

as:

}

Wt, θθθt
{

}

W(n)
l

+ α(n)V, (40)

(cid:17)
+ α(n)v,

2

γ

2

(cid:17)

r(n)
w,tl
(cid:16)
r(n)
θ,tl

W(n)
(cid:16)
θθθ(n)
t −
(cid:17)
(cid:16)
and r(n)
θ,tl =

t −
θθθ(n)
l

W(n+1)
l
θθθ(n+1)
l

= W(n)

l + β0e−
γ

= θθθ(n)

(cid:16)

||

||

(41)

t −

(W(n)

(cid:17)
(θθθ(n)

w,tl =

l + β0e−
θθθ(n)
W(n)
where r(n)
are the
l
l
Cartesian distances, β0 is the attractiveness at r(n)
w,tl = 0 and
r(n)
θ,tl = 0, γ presents the variation of of the attractiveness. The
second terms of (40) and (41) capture the attractions while
the third terms of (40) and (41) are randomization comprised
of randomization factor α(n), V
1. The
factor α(n), the elements of V and v are drawn from either an
uniform or a Gaussian distribution.

U and v

CMt ×

CMt×

t −

∈

∈

||

||

It can be observed that problem (35) is a special case of the
proposed framework (1) where the objective and constraints
are functions of optimization variables W and θθθ. The proposed
FA for RIS has the same steps as those in Algorithm 1 except
steps 3, 16, 18 and 19 given in Algorithm 5.

Algorithm 5 Modiﬁed generalized FA for solving (32)

i , ηi, Ito;

Input: FA parameters: N, T , λi, ρn, β0; γ; Optimization
data: H, gi, σ2
Step 3: Evaluate the light intensities of N ﬁreﬂies as (39);
Step 16: Move ﬁreﬂy i towards ﬁreﬂy j as (40) and (41);
r(n)
Step 18: Attractiveness varies with distances via e−
w, ji
(cid:16)
and e−
Step 19: Evaluate new solutions; update Ii (Wi, θθθi) as (39);
return W⋆, θθθ⋆.

r(n)
θ, ji
(cid:16)

;

γ

γ

(cid:17)

(cid:17)

2

2

C. Complexity Analysis

Here, we analyze the computational complexities of the AO

and the proposed FA for RIS-aided beamforming problem.

Lemma 5: The complexity of the AO approach is on the

order of:

where

no (τ1 + τ2) ,

(42)

τ1 = ln

1

ε−

U(Mt + 1)

t + 1)U + U M2

t (M2

t + Mt)

(M2
h

(43)

N2

t .(44)

i

(N2

t + 1)(U + 2N2

t ) + N4
t

(cid:16)

(cid:17) p

h

Proof: We ﬁrst give some hints to derive the computa-
tional complexity of obtaining optimal solution to problems
(33) and (34). With the observation that (33) has U LMI
constraints of size 1 and U LMI constraints of size Mt, one
can follow the same steps as in [41, Section V-A] to derive
the complexity of solving (33) as τ1 given in (43).

At a given ε > 0, Θε is called an ε-solution to problem (34)
Tr (Θ)+ε. The number of decision variables of

if Tr (Θε)

min
Θ

≤

(37)

(cid:16)
+M4
t

(cid:17) p
M2
t ,

τ2 = ln

i
1
ε−

U + 2Nt

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

9

(34) is N2
t . Observing that (34) has U linear-matrix-inequality
(LMI) constraints of size 1 and 2 LMI constraints of size
Nt, one can derive the computational complexity to attain ε-
solution to (34) as the order of τ2 given in (44).

Since the AO approach iteratively solves (33) and (34) in
no iterations, the complexity of AO approach is on the order
of no (τ1 + τ2).

Lemma 6: The computational complexity of Algorithm 5 is

on the order of

the l-th iterations is calculated as
beamforming vector at
U
1)θθθ(l
i=1 αiGiθθθ(l
w(l) = √Peigmax
where eigmax (X) is
−
−
the maximum eigenvalue of matrix X. The k-th coeﬃcient of
the RIS’s phase shift vector at the l-th iterations is calculated
if µk , 0, where
θθθ(l)
as
h
µk =

= 1 if µk = 0 and
i w(l)w(l)HGiθθθ(l
−

k
U
i=1 αiGH

θθθ(l)
k
i
h
1)
.

= µk
µk|
|

1)HGH
i

(cid:16)P

(cid:17)

k
i

i
hP

B. Proposed Fireﬂy Algorithm

The optimization (47) can be expressed as

M2

t + Nt + N

U Mt + U(N2

T N2
(cid:16)
+T N log N + N MtU + NtN + N log N
+N

h
U Mt + U(N2

t + MtNt) + Nt

.

t + MtNt) + Nt

(cid:17)i

(45)

min
W, θθθ
{
}
s. t.

(cid:16)

(cid:17)

U
i=1 wH

Proof: The proof is based on the following observa-
tions. The dominant terms of the computational complexity
of Algorithm 5 are at steps 2, 3, 4, 16, 19, and 22. The
complexity of generating N ﬁreﬂies in step 2 is on the order of
N MtU + NtN. The complexities of evaluating φi(W, θθθ), ϕk(θk),
i wi are, respectively, on the order of U(N2
t + MtNt),
and
Nt, and U Mt. Hence, the complexity of calculating the light
density for N ﬁreﬂies, i.e., steps 3 and 19, is on the order of
. The complexity of ranking N
N
ﬁreﬂy in steps 4 and 22 is N log N. Finally, the complexity of
(cid:17)
moving a ﬁreﬂy in step 16 is on the order of M2
t +Nt. Assuming
a worst case when step 16 is executed in every inner loop of
the algorithm, after some manipulations, one can arrive at (45).

t + MtNt) + Nt

U Mt + U(N2

P

(cid:16)

VI. RIS-Aided Wireless Power Transfer

A. Problem Formulation

1) Problem Formulation: Consider a similar communica-
tion system in V-A, however, the users are energy harvesting
receivers (EHRs) instead of information decoding receivers.
Using the same notations as in V-A, the power arrived at the i-
th user is:

Ei =

i diag(θθθ)HHH
gH

U

Xj=1

w j

U

2

=

Xj=1

wH

j Giθθθ θθθHGH

i w j,(46)

where w j is the active energy beamforming vector for the j-th
user. we interested in maximizing a total weighted sum power
received at the EHRs obtained via the following optimization
problem:

(cid:12)(cid:12)(cid:12)(cid:12)

(cid:12)(cid:12)(cid:12)(cid:12)

max
, θθθ
wi}
{

s. t.

αiwH

j Giθθθ θθθHGH

i w j

U

U

Xj=1

Xi=1
U

wH

j w j

P,

θk

|

|

≤

= 1,

k,

∀

Xj=1

(47)

where P is the maximum transmit power of the BS and αi
is the weighting factor for the i-th EHR.

0

≥

2) Successive Convex Approximation: According to [23],
for any ﬁx θθθ, only one common energy beam is suﬃcient.
Using a successive convex approximation (SCA) technique,
[23] proposed an iterative algorithm to ﬁnd optimal active
and passive beamforming vectors for problem (47) as follows.
Starting with an initialized value θθθ(0),
the optimal active

f (W, θθθ)

−
W, θθθ
φ (
≤
{
ϕk (θk) = 0,
∀

)
}

0,
k,

(48)

where W =
U
j=1 αiwH
θk

U
i=1
ϕk (θk) =
P
P
|
written as:

| −

f (W, θθθ) =
w1, w2,
j GiθθθθθθHGH
h
P, and
1. Adopting the penalty method, (35) can be

, wU
i w j, φ (W, θθθ) =

CMt ×
U,
U
j=1 wH

j w j

· · ·

−

∈

i

P

min
W, θθθ
}
{

−

f (W, θθθ) + P(W, θθθ)

(49)

Let

Nt
k=1 ρk

0, φ(
{
1, wt
2,

2 +
where P(W, θθθ) = λmax
}
λ > 0 and ρk > 0 are penalty constants.
, θθθt
}
Wt, θθθt
{

W, θθθ
)
}
{
, wt
be the ﬁreﬂy t. We
U
{h
initialize a population of N ﬁreﬂies
, N
1, 2,
i
}
and deﬁne the light density, i.e., the brightness, of the ﬁreﬂy
t

2, with
}

ϕk(θk)
{

Wt, θθθt
{

, t
}

∈ {

· · ·

· · ·

wt

as:

P

=

}

Wt, θθθt
{

}

It (Wt, θθθt) =

−

1
f (Wt) + P(Wt, θθθt)

.

(50)

It can be observed that problem (48) is a special case of the
proposed framework (1) where the objective and constraints
are functions of optimization variables W and θθθ. Utilizing the
ﬁreﬂy movements deﬁne in (40) and (41) in SectionV-B, the
proposed FA for RIS has the same steps as those in Algo-
rithm 1 except steps 3, 16, 18 and 19 given in Algorithm 6.

Algorithm 6 Modiﬁed generalized FA for solving (47)

Input: FA parameters: N, T , λ, ρk, β0; γ; Optimization
data: H, gi, αi, P;
Step 3: Evaluate the light intensities of N ﬁreﬂies as (50);
Step 16: Move ﬁreﬂy i towards ﬁreﬂy j as (40) and (41);
r(n)
Step 18: Attractiveness varies with distances via e−
w, ji
(cid:16)
and e−
Step 19: Evaluate new solutions; update Ii (Wi, θθθi) as (50);
return W⋆, θθθ⋆.

r(n)
θ, ji
(cid:16)

;

γ

γ

(cid:17)

(cid:17)

2

2

C. Complexity Analysis

Here, we analyze the complexities of the SCA approach
and the proposed FA for the RIS-aided WPT beamforming.
We start by introducing the following lemma.

Lemma 7: The complexity of the SCA approach is on the

order of:

m0

U Mt (Mt + Nt) + M3

t + Mt log Mt + N3

t + N2

t Mt

,

(51)

(cid:16)

where m0 is the number of iterations of the SCA approach.

(cid:17)

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

10

M2

1)θθθ(l
−

1)θθθ(l
−

1)H GH

1)HGH
i

t + MtNt

is on the order of U

Proof: At each iteration, the complexity of evaluating
αiGiθθθ(l
. The
−
complexities of ﬁnding a maximum eigenvalue of the Mt
Mt
(cid:17)
(cid:16)
matrix αiGiθθθ(l
i based on the SVD method is on the
−
t + Mt log Mt. Hence, the complexity of ﬁnding w(l)
order of M3
is on the order of U Mt(Mt +Nt)+ M3
t + Mt log Mt. Furthermore,
the complexity of calculating µk is on the order of N2
t + MtNt.
Therefore, the complexity of ﬁnding θθθ(l) is on the order of
. Consequently, m0 iterations of evaluating w(l)
t + MtNt
Nt
and θθθ(l) lead to (51).

(cid:16)
Lemma 8: The complexity of the Algorithm 6 is on the

N2

×

(cid:17)

order of:

(c)

FA
Iterative

35

30

25

20

15

10

]

m
B
d
[

r
e
w
o
p

t
i
s
m
n
a
r
T

(a)

FA
Iterative

35

30

25

20

15

10

]

m
B
d
[

r
e
w
o
p

t
i
s
m
n
a
r
T

(b)

FA
Iterative

35

30

25

20

15

10

]

m
B
d
[

r
e
w
o
p

t
i
s
m
n
a
r
T

M2

t + Nt + N

U Mt + U(N2

T N2
(cid:16)
+T N log N + N MtU + NtN + N log N
+N

h
U Mt + U(N2

t + MtNt) + Nt

.

t + MtNt) + Nt

(cid:17)i

(52)

5

0

10
SINR [dB]

20

5

0

10
SINR [dB]

20

5

0

10
SINR [dB]

20

(cid:16)

(cid:17)

Proof: Noticing that

the complexities of evaluating
φ(W, θθθ), ϕk(θk), and f (W, θθθ) are, respectively, on the order
Nt Mt + N2
of U Mt, Nt, and U
. One can easily show that
t
the complexity of the Algorithm 6 is the same as that of the
Algorithm 5.

(cid:16)

(cid:17)

VII. Numerical Results

In this section, we perform simulations to evaluate the per-
formances of the proposed FA approaches, i.e., FA approaches
for transmit beamforming, cognitive cognitive beamforming,
RIS-aided transmit beamforming, and RIS-aided WPT, and
compare them with their iterative, SDP, and SCA counterparts.
CVX package [40] is utilized to obtain the solution for
the cognitive SPD approach, i.e., problem (21), and the AO
approach for the RIS-aided transmit beamforming. In the AO
approach, two SDPs (33) and (34) are alternatively solved
in n0 = 10 iterations. The setup parameters for FAs are as
follows. The variation of the attractiveness γ is set at 1. The
penalty constants are set equal but they dynamically vary
as λi = ρk = n2,
i, k where n is the generation index in
Algorithm 1. The attractiveness at zero distance is β0 = 1.
Finally, the initial randomization factor is α(0) = 0.9 and its
value at the n-th generation is α(n) = α(0)0.9n.

∀

A. Evaluation on Transmit Beamforming

−

We simulate a scenario of two users, i.e., U = 2, randomly
distributed within 2 km from their BS. The array antenna
gain at the BS is 15dBi. The noise power spectral density,
noise ﬁgure at each user and the subcarrier bandwidth are,
174 dBm/Hz, 5 dB and 15 kHz wide. The path
respectively,
loss model is 35 + 34.5 log 10(l), where l is in kilometers. A
log-normal shadowing with a standard deviation of 8 dB is
assumed. Furthermore, a complex Gaussian distribution is set
with the variance of 1/2 on each of its real and imaginary
components for the downlink channel fading coeﬃcients.
Monte Carlo simulations have been carried out over 1000
channel realizations.

Fig. 1 illustrates the total transmit power of the proposed
FA approach and its iterative counterpart versus the required
SINR level with diﬀerent numbers of BS’s antennas. The

Fig. 1: The total BS’s transmit power versus the required SINR
level with diﬀerent numbers of BS’s antennas: (a) 4 antennas; (b)
6 antennas; (c) 8 antennas. The ﬁreﬂy population is N = 30. The
number of maximum generations T = 30.

results on Fig. 1 clearly show that the proposed FA approach
outperforms the iterative method in obtaining lower required
transmit power, i.e., around 3 to 4 dB lower, for all simulated
setups. The results in Fig. 1 conﬁrm the ability of the proposed
FA in handling highly nonlinear and multimodal optimization
problems. This power saving gain, however, comes at the price
of a higher complexity. Using the parameter setup for Fig. 1 in
Lemmas 1 and 2, i.e., U = 2, T = N = 30, Mt = 4, 6, 8, one
can ﬁnd the complexities of the Iterative and FA approaches
. When the
are, respectively, in the order of
number of antennas elements are large, letting T = N = Mt,
(cid:17)
it can be shown that the dominant terms of the complexities
M4
of the Iterative and FA approach are in the order of
t
(cid:17)
, respectively. The trade oﬀ between the power
and
saving gain and computational complexity of the proposed FA
approach in comparison with the Iterative method should be
considered by the network designer/operator.

O (cid:16)

O (cid:16)

O (cid:16)

O (cid:16)

M6
t

104

108

and

(cid:17)

(cid:17)

Fig. 2 shows the total BS’s transmit power of the Iterative
and proposed FA versus the number of iteration/generations
with diﬀerent numbers of BS’s antennas. The results indi-
cate that the Iterative approach converges after just 5 iter-
ations/generations while the proposed FA requires about 20
generations/iterations to level oﬀ.

Fig. 3 shows the total BS’s transmit power of the proposed
FA approach versus the number of population N with diﬀerent
BS’s antenna elements. It can be seen that the observed curves
converge after N = 30. Our simulations indicate that the
proposed FA approach performs well with at least 30 ﬁreﬂies
to solve (12) under the investigated SINR range.

B. Evaluations on Cognitive Transmit Beamforming

We ﬁrst reproduce the result of the experiment described in
Example 1 of [3] to compare the proposed FA approach with
the SDP approach. In that experiment, three SUs are located
5◦, 10◦, 25◦, and two PUs are located at 30◦ and 50◦,
at

−

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

11

40

35

30

25

20

15

10

5

]

m
B
d
[

r
e
w
o
p
t
i
s
m
n
a
r
T

(a)

FA
Iterative

40

35

30

25

20

15

10

5

]

m
B
d
[

r
e
w
o
p
t
i
s
m
n
a
r
T

(b)

FA
Iterative

40

35

30

25

20

15

10

5

]

m
B
d
[

r
e
w
o
p
t
i
s
m
n
a
r
T

(c)

FA
Iterative

-5

-10

-20

B
d

-30

(a): SDP Approach

-40

-90 -80 -70 -60 -50 -40 -30 -20 -10 0

10 20 30 40 50 60 70 80 90

Azimuth in degrees
(b): FA Approach

-5

-10

-20

B
d

-30

-40

-90 -80 -70 -60 -50 -40 -30 -20 -10 0

10 20 30 40 50 60 70 80 90

Azimuth in degrees

0

10

0
30
Generations/Iterations

20

0

10

0
30
Generations/Iterations

20

0

10

0
30
Generations/Iterations

20

Fig. 2: The total BS’s transmit power versus the generations/iteration
with diﬀerent numbers of BS’s antennas: (a) 4 antennas; (b) 6
antennas; (c) 8 antennas. The ﬁreﬂy population is N = 30. The
required SINR level at each user is 10 dB.

Fig. 4: The radiation pattern of the BS with 8 antennas: (a) The
reproduction of [3, Fig. 3]; (b) The proposed FA approach with the
number of population N = 100.

19.6

19.55

19.5

]

m
B
d
[

r
e
w
o
p
t
i
s
m
n
a
r
T

10

20

17.96

17.94

17.92

10

20

16.9

16.85

10

20

(a)

30

40
Number of population [N]
(b)

50

30

40
Number of population [N]
(c)

50

30

40
Number of population [N]

50

60

70

60

70

60

70

Fig. 3: The total BS’s transmit power versus the number of population
with diﬀerent numbers of BS’s antennas: (a) 4 antennas; (b) 6
antennas; (c) 8 antennas. The number of generation is T = 30. The
required SINR level at each user is 10 dB.

relative to the BS’s array broadside. The tolerable interference
level two PUs are Ito,1 = 0.001 and Ito,2 = 0.0001. The noise
variance is set to 0.1 while the required SINR values are set
to 1 for the SUs.

The channel covariance matrices from the secondary BS
, and to PU k, i.e., Rp,k =
to SU t , i.e., Rs,t = R
R
, are the function of the angle of departure, i.e., ζs,t
(cid:1)
or ζp,k, and the standard deviation of the angular spread, i.e.,
(cid:17)
δa. The (m, n)th entry of R (ζ, δa) is, [20]:

ζs,t, δa
(cid:0)

ζp,k, δa
(cid:16)

e

j2π∆
ψ [(n
−

m)sinζ]e−

2
h
where ψ is the carrier wavelength, σa = 2◦, and the antenna
spacing at the BS is set as ∆ = ψ/2.

(53)

m)cosζ

π∆δa
ψ {

}i

(n

−

,

2

Fig. 4 (a) illustrates the radiation patterns at the BS of the
SDP approach as described in (21), which is the reproduction

of Fig. 3 in [3], while Fig. 4 (b) shows the radiation patterns
the BS of the FA approach proposed in Algorithm 4.
at
The results clearly indicate that the FA obtains the same
radiation pattern as the SDP approach does. Both approaches
are able to form nulls to the locations/angles where the PUs are
located. In other words, the proposed FA can obtain the same
optimal solution as the IPM does for the SDP counterpart. This
conﬁrms the ability of the proposed FA in handling highly
nonlinear and multimodal optimization problems.

With the setup in Fig. 4, i.e., Mt = 8, U = 3, K = 2, N = 100
and, T = 80, one can easily verify from Lemmas 3 and 4
that the proposed FA approach requires higher computational
complexity than the SDP approach does when it returns rank-
one optimal solution. When the number of antennas is large,
one can show that the dominant term of (28) is M6 1
. On the
other hand, assuming T = N = Mt, the dominant term of
(29) is M6
t . Hence, the complexity of an IPM to solve (21)
is slightly higher than the complexity of the proposed FA in
Algorithm 4, i.e.,

in comparison with

M6 1

t

2

2

Fig. 5 shows the transmit power of the proposed FA ap-
proach versus the number of population with diﬀerent numbers
of transmit antennas. The results indicate that the proposed
FA converges with all number of antenna setups as all the
observed curves level oﬀ after the maximum size of population
of N = 50. However, the higher of the antenna elements is, the
larger the size of the population is required for a converged
transmit power. For example, with M = 8, 16, and 32, the
proposed FA approach, respectively, obtains a stable transmit
power at N = 30, 40 and 50. This is due to the fact that
the size of the system increases with a higher number of
antenna elements, i.e., a higher degree of freedom. As a result,
it requires a larger size of the population to provide a suﬃcient
diversiﬁcation for the exploration of the FA. The results also
show that the required transmit power decreases when the
number of antennas increase as the result of having higher
degree of freedom.

M6
t

.
(cid:17)

O (cid:16)

O (cid:18)

t

(cid:19)

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

12

10

5

0

-5

-10

-15

]

B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

-20

10

20

30

50

70
40
Number of population N

60

M=8
M=16
M=32

80

90

100

40

30

20

10

]

m
B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

0

0

30

20

10

]

m
B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

0

0

Mt=3, Nt=30

FA
AO

4

8

12

16

20

Required SINR level [dB]
Mt=8, Nt=30

FA
AO

4

8

12

16

20

Required SINR level [dB]

40

30

20

10

]

m
B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

0

0

30

20

10

]

m
B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

0

0

Mt=3, Nt=20

FA
AO

4

8

12

16

20

Required SINR level [dB]
Mt=8, Nt=20

FA
AO

4

8

12

16

20

Required SINR level [dB]

Fig. 5: The total transmit power of the proposed FA approach versus
the number of population with diﬀerent numbers of transmit antennas.
The number of maximum generation T = 150.

Fig. 7: The total BS’s transmit power versus the required SINR
level with diﬀerent numbers of BS’s antennas and RIS’s reﬂective
elements. The ﬁreﬂy population is N = 120. The number of maximum
generations T = 50.

M=8
M=16
M=32

]

B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

50

40

30

20

10

0

-10

-20

0

20 40 60 80 100 120 140 160 180 200 220 240 260 280 300

Number of generations

Fig. 6: The total
transmit power of the proposed FA approach
versus the number of maximum generations with diﬀerent numbers
of transmit antennas. The number of population N = 70.

Fig. 6 depicts the transmit power of the proposed FA
approach versus the number of maximum generations with
diﬀerent numbers of transmit antennas. A similar trend as in
Fig. 5 is also observed in this ﬁgure. The transmit power
attained by the proposed FA approach converges with all
numbers of antenna setups. The higher number of antennas
is, the higher number of generations is needed as a result of
higher exploitation required for the increase of the problem
the transmit power levels oﬀ at
dimension. For instance,
around 90, 100, and 120 generations, respectively, for M = 8,
16, and 32.

C. Evaluations on RIS-aided Transmit Beamforming

We simulate a RIS-aided communication system which
consists of one BS, one RIS, and two users, i.e., U = 2.
The distance between the BS and the RIS is 10 m. Users

−

−

−

30

are randomly distributed with a distance of 6 m from the RIS.
The pathloss exponents of both wireless links from the BS
to the RIS and from the RIS to users are set to be 2.2 with
the signal attenuation at the reference distance of 1 m being
30 dB [23], i.e., the large-scale fading coeﬃcient is modeled
22 log10(d) dB where d is the distance between the
as
BS to RIS or RIS to a user. The noise variance at each user
is
124 dBm. Monte Carlo simulations are carried over 100
channel realizations. Each channel realization is associated
with a random user location and a random fading coeﬃcient.
Fig. 7 illustrates the total BS’s transmit power versus the
required SINR level with diﬀerent numbers of BS’s antennas
and RIS’s reﬂective elements. The results indicate that the
proposed FA prevails the AO approach in terms of lower power
consumption. The superior performance of the FA approach
over its AO counterpart can be explained as follows. As the
AO approach approximates non-convex problem (32) by two
convex sub-problems (33) and (34), the solution obtained by
the AO approach is not necessary the global optimal solution
of the original problem (32). On the other hand, the proposed
FA possessing both exploitation and exploration abilities can
eﬀectively handle such non-convex problem and obtain much
better solution than its counterpart. The results shown on
Fig. 7 verify the ability of the proposed FA in handling highly
nonlinear and multimodal optimization problems.

It can be observed from Fig. 7 that at a given number of
RIS’s reﬂective elements, the performance gap between the
proposed FA and the AO decreases when the number of BS’s
antennas increases. For example, when Nt = 20, the gaps are,
respectively, around 7.5 dB and 3.5 dB with Mt = 3 and
Mt = 8. Fortunately, at a given number of BS’s antennas, the
performance gap improves when the number of RIS’s elements
increases. For instance, with Mt = 8, the performance gap
increases from around 3.5 dB to 4.5 dB when Nt increases
from 20 to 30. Interestingly, the FA performs especially well
with a relatively high ratio of Nt/Mt, i.e., the performance gap

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

13

26

24

22

20

18

16

14

12

]

m
B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

10

0

20

40

Mt:8, Nt:30
Mt:8, Nt:20
Mt:3, Nt:30
Mt:3, Nt:20

34

32

30

28

26

24

22

]

m
B
d
[

r
e
w
o
p

t
i

m
s
n
a
r
T

160

180

200

20

20

40

60

Mt:8, Nt:30
Mt:8, Nt:20
Mt:3, Nt:30
Mt:3, Nt:20

160

180

200

100

140
80
Number of Population (N)

120

60

100
Maximum number of generations (T)

120

140

80

Fig. 8: The total BS’s transmit power versus the number of maximum
generations with diﬀerent numbers of BS’s antennas and RIS’s
reﬂective elements. The ﬁreﬂy population is N = 120. The required
SINR level is 10 dB.

Fig. 9: The total transmit power versus the number of populations
with diﬀerent numbers of BS’s antennas and RIS’s reﬂective ele-
ments. The number of maximum generations T = 50. The required
SINR level is 20 dB.

is around 9.5 dB with the ration of 30/3 while it is around
3.5 with the ratio of 20/8. The results can be explained as
follows. A higher number of RIS’s reﬂective elements gives
more degree of freedom for the FA to perform. Moreover, the
channel between the RIS and these users plays a higher role
than that between the BS and the RIS does as the former is
closer to these users. Last but not least, the performance gaps
slightly decrease at relatively high SINR level especially when
the Nt/Mt ratio is relatively low. For example with the ratio of
20/8, the performance gap is around 1.8 dB at SINR of 20 dB
compared with around 3.5 dB at the other SINR levels, i.e.,
see the bottom-right corner ﬁgure of Fig. 7. This is because
of a fact that the FA has reached its limit of exploration with
N = 120 ﬁreﬂies, at a stricter constraint condition.

We now compare the computational complexities of the AO
and FA approaches for the experiments presented on Fig. 7.
As Nt is larger than Mt, from Lemma 5 one can show that
the dominant term of the complexity of the AO approach
6 1
is n0N
. Similarly, from Lemma 6 one can conclude that
2
t
the dominant term of the complexity of the FA approach is
T N3N2
t . Substituting for Nt = 30, n0 = 10, N = 120 and
T = 50, we can arrive at the fact that the computational
complexities of the AO and FA approaches are on the same
. When the numbers of antennas Mt and Nt
order of
are large, letting Nt = n0 = Mt in (42), one can show that the
(cid:17)
dominant term of the complexity to attain ε-solution to (32)
is M7 1
. On the other hand, one can derive the dominant term
of (45) as M6
t when assuming T = N = Nt = Mt. Hence,
the complexity of an IPM to solve (32) is higher than the
M7 1
complexity of the proposed FA in Algorithm 5, i.e.,

1010

O (cid:16)

t

2

2

in comparison with

M6
t

.

O (cid:16)

(cid:17)

In Fig. 8, the total BS’s transmit power is plotted versus
the maximum of generation T used in the FA in Algorithm 5
with diﬀerent BS’s antennas and RIS’s reﬂective elements. The
results indicate that the proposed FA requires around 50 to 60

O (cid:18)

t

(cid:19)

generations to attain the optimal solution for all setups.

Fig. 9 illustrates the total transmit power versus the number
of population N with diﬀerent BS’s antennas and RIS’s
elements. The results show that increasing the size of the
ﬁreﬂy population enables the FA to obtain better solution.
For example, the total transmit power decreases around 7
dB, 5.4 dB, 5 dB, and 3 dB, respectively, for the setups of
(Mt = 8, Nt = 20), (Mt = 3, Nt = 30), (Mt = 8, Nt = 20),
and (Mt = 3, Nt = 20) when the ﬁreﬂy population increases
from 20 to 120. The performance gap at the 20 dB SINR level
observed in Fig. 7 for (Mt = 8, Nt = 20) can be improved 1 dB
further when the population size is enlarged from 120 to 200.
These total-transmit-power curves converge after N = 180 as
the reduction in the total transmit power is negligible when
the population increases to N = 200 for all setups.

D. Evaluations on RIS-aided WPT

Here, we use the same setup for the RIS-aided commu-
nication system as considered in the previous section, i.e.,
Section VII-C. However, the EHRs are randomly placed with
the distance of 2 m from the RIS. We run m0 = 10 iterations
to obtain the solution for the SCA approach.

Fig. 10 shows the sum-power received at EHRs versus
BS’s maximum transmit power with diﬀerent numbers of BS’s
antennas and RIS’s reﬂective elements. It is clear from the
ﬁgure that the proposed FA approach outperforms the SCA
approach in [23] in oﬀering higher sum-power at EHRs. The
performance gaps are, respectively, around 18 dB, 17 dB,
15 dB, and 14 dB for the setups of (Mt = 3, Nt = 30),
(Mt = 8, Nt = 30), (Mt = 3, Nt = 20), and (Mt = 8, Nt = 20).
The superior performance of the proposed FA over the SCA
is due to the advantage of having exploitation and exploration
abilities to handle non-convex optimization problems. On the
other hand, the SCA employs the ﬁrst-oder Taylor expansion
to approximate the optimization problem resulting in a lower-
bounded solution. Furthermore, the FA approach allocates

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

14

]

m
B
d
[

s
R
H
E

t
a

r
e
w
o
p
-
m
u
S

]

m
B
d
[

s
R
H
E

t
a

r
e
w
o
p
-
m
u
S

Mt=3, Nt=30

FA
SCA

20

30

40

]

m
B
d
[

s
R
H
E

t
a

r
e
w
o
p
-
m
u
S

-80

-90

-100

-110

-120

10

Mt=3, Nt=20

FA
SCA

20

30

40

-80

-90

-100

-110

-120

10

Maximun transmit power at BS [dBm]
Mt=8, Nt=30

-80

-90

-100

-110

-120

10

FA
SCA

20

30

40

Maximun transmit power at BS [dBm]
Mt=8, Nt=20

-80

-90

-100

-110

-120

10

FA
SCA

20

30

40

]

m
B
d
[

s
R
H
E

t
a

r
e
w
o
p
-
m
u
S

Maximun transmit power at BS [dBm]

Maximun transmit power at BS [dBm]

]

m
B
d
[

s
R
H
E

t
a
r
e
w
o
p
-
m
u
S

-80

-85

-90

-95

-100

-105

0

20

40

Mt:8, Nt:30
Mt:8, Nt:20
Mt:3, Nt:30
Mt:3, Nt:20

60

100
Maximum number of generations (T)

140

120

80

160

180

200

Fig. 10: Sum-power received at EHRs versus BS’s maximum transmit
power with diﬀerent numbers of BS’s antennas and RIS’s reﬂective
elements. The ﬁreﬂy population is N = 100. The number of maximum
generations T = 50.

Fig. 11: Sum-power received at EHRs versus the number of maximum
generations with diﬀerent numbers of BS’s antennas and RIS’s
reﬂective elements. The ﬁreﬂy population is N = 100. The required
SINR level is 10 dB.

one active beamforming vector for each EHR whereas the
SCA only uses one active beamforming vector for all EHRs.
The results shown on Fig. 10 again verify the ability of the
proposed FA in handling highly nonlinear and multimodal
optimization problems.

Comparing Figs. 7 and 10, it can be observed that the
FA behaves in a similar manner for both power minimization
problem (35) and sum-power maximization problem (48). For
instance, at the same value of Mt, the higher the value of
Nt, the larger the performance gap is. At the same value of
Nt, the lower the value of Mt, the bigger the performance
gap is. The results also recommend to maintain a relatively
high ratio of Nt/Mt to attain the best performance of the FA.
Slight declines in the performance gaps are also observed at
the stricter constraint of BS’s transmit power, i.e., 40 dBm, as
the FA’s population reach their limit of exploration.

We proceed by comparing the computational complexities
of the SCA and FA approaches for the experiments shown on
Fig. 10. As Nt is larger than Mt, from Lemmas 7 and 8, it is
clear that the dominant terms of the complexities of the SCA
t and T N3N2
and the FA approaches are, respectively, m0N3
t .
Substituting for Nt = 30, m0 = 10, N = 100 and T = 50, we
can arrive at the fact that the computational complexities of
the SCA and FA approaches are, respectively, on the orders of
. When the numbers of antennas Mt and
O (cid:16)
Nt are large, letting Nt = m0 = Mt in (51), one can show that
(cid:17)
the dominant term of the complexity of the SCA is M4
t . On the
other hand, the dominant term of (52) is M6
t when assuming
T = N = Nt = Mt. Hence, the complexity of the SCA approach
is lower than that of the proposed FA in Algorithm 6, i.e.,

1010

O (cid:16)

105

and

(cid:17)

(cid:17)

.
(cid:17)

in comparison with

M6
M4
t
t
O (cid:16)
O (cid:16)
Sum-power received at EHRs are shown versus the number
of maximum generations with diﬀerent numbers of BS’s
antennas and RIS’s reﬂective elements in Fig. 11. The ﬁgure
reveals that the proposed FA converges after around 50 to 60
generations for all observed setups.

]

m
B
d
[

s
R
H
E

t
a

r
e
w
o
p
-
m
u
S

-83

-84

-85

-86

-87

-88

-89

-90

-91

-92

-93

0

20

40

Mt:8, Nt:30
Mt:8, Nt:20
Mt:3, Nt:30
Mt:3, Nt:20

80

120
60
Number of Population (N)

100

140

160

180

Fig. 12: Sum-power received at EHRs versus the number of popu-
lations with diﬀerent numbers of BS’s antennas and RIS’s reﬂective
elements. The number of maximum generations T = 50. The required
SINR level is 20 dB.

The eﬀect of the ﬁreﬂy population on the sum-power
received at EHRs is illustrated on Fig. 12. The ﬁgure shows
that all the curves converge after the population size of 80.
However the diﬀerence between the EHRs’ sum-power oﬀered
by 80 ﬁreﬂies and that oﬀered by 40 ﬁreﬂies is no more
than 0.7 dB for all observed setups. This indicates that the
complexity of the proposed FA for the RIS-aided WPT sum-
power maximization problem in (48) can be reduced with an
acceptable tradeoﬀ in the optimality.

VIII. Conclusion
We have proposed a generalized FA to ﬁnd optimal solution
for an optimization framework containing objective function
and constraints as multivariate functions of independent opti-
mization variables. We have adopted the proposed generalized

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

15

FA to solve four representative examples of classic trans-
mit beamforming, cognitive beamforming, RIS-aided transmit
beamforming, and RIS-aided wireless power transfer. Our
analyzes have indicated that the computational complexities
of proposed FA approaches are less than those of their IPM
counterparts, i.e., the SDP and the AO approaches, yet higher
than that of the iterative and SCA approaches in large-antenna
scenarios. Simulation results have revealed the fact that the
proposed FA attains the same optimal solution as the IMP
does for the under-investigated cognitive beamforming prob-
lem. Interestingly, the proposed FA outperforms the iterative,
AO, and SCA approaches for the under-investigated classic
transmit beamforming, RIS-aided transmit beamforming, and
wireless power transfer problems, respectively. This conﬁrms
the eﬀectiveness of the proposed generalized FA in handling
multivariate and non-convex problems.

References

[11] Y. Huang and D. P. Palomar, “Rank-constrained separable semideﬁnite

programming with applications to optimal beamforming,” IEEE Trans.

Signal Process., vol. 58, no. 2, pp. 664– 678, Feb. 2010.

[12] Y. Huang, Q. Li, W.-K. Ma, and S. Zhang, “Robust multicast beamform-

ing for spectrum sharing-based cognitive radios,” IEEE Trans. Signal

Process., vol. 60, no. 1, pp. 527– 533, Jan. 2012.

[13] B. Clerckx, R. Zhang, R. Schober, D. W. K. Ng, D. I. Kim, and H. V.

Poor, “Fundamentals of wireless information and power transfer: From

RF energy harvester models to signal and system designs,” IEEE J. Sel.

Areas in Commun., vol. 37, no. 1, pp. 4–33, Jan. 2019.

[14] D. W. K. Ng, E. S. Lo, and R. Schober, “Robust beamforming for

secure communication in systems with wireless information and power

transfer,” IEEE Trans. Wireless Commun., vol. 13, no. 8, pp. 4599–4615,

Aug. 2014.

[15] ——, “Wireless information and power transfer: Energy eﬃciency opti-

mization in OFDMA systems,” IEEE Trans.Wireless Commun., vol. 12,

no. 12, pp. 6352–6370, Dec. 2013.

[1] F. Rashid-Farrokhi, K. J. R. Liu, and L. Tassiulas, “Transmit beamform-

[16] T. A. Le, Q.-T. Vien, H. X. Nguyen, D. W. K. Ng, and R. Schober,

ing and power control for cellular wireless systems,” IEEE J. Sel. Areas

“Robust chance-constrained optimization for power-eﬃcient and secure

Commun., vol. 16, no. 8, pp. 1437– 1450, Oct. 1998.

SWIPT systems,” IEEE Trans. Green Commun. and Netw., vol. 1, no. 3,

[2] A. Wiesel, Y. C. Eldar, and S. Shamai, “Linear precoding via Conic

pp. 333–346, Sep. 2017.

optimization for ﬁxed MIMO receivers,” IEEE Trans. Signal Process.,

[17] W. Yu and T. Lan, “Transmitter optimization for the multi-antenna

vol. 54, no. 1, pp. 161– 176, Jan. 2006.

downlink with per-antenna power constraints,” IEEE Trans. Signal

[3] Y. Huang and D. P. Palomar, “Rank-constrained separable Semideﬁnite

Process., vol. 55, no. 6, pp. 2646–2660, Jun. 2007.

programming with applications to optimal beamforming,” IEEE Trans.

[18] T. A. Le and K. Navaie, “Downlink beamforming in underlay cognitive

Signal Process., vol. 58, no. 2, pp. 644–678, Feb. 2010.

cellular networks,” IEEE Trans. Commun., vol. 62, no. 7, pp. 2212–2223,

[4] H. Dahrouj and W. Yu, “Multicell interference mitigation with joint

Jul. 2014.

beamforming and common message decoding,” IEEE Trans. Commun.,

[19] Z.-Q. Luo, W.-K. Ma, A. M.-C. So, Y. Ye, and S. Zhang, “Semideﬁnite

vol. 59, no. 8, pp. 2264–2273, Aug. 2011.

relaxation of quadratic optimization problems,” IEEE Signal Process.

[5] W. Yang and G. Xu, “Optimal downlink power assignment for smart

Mag., vol. 27, no. 3, pp. 20–34, May 2010.

antenna systems,” in Proc. IEEE Int. Conf. Acoustics, Speech and Signal

[20] M. Bengtsson and B. Ottersten, “Optimal downlink beamforming using

Process., ICASSP ’98, vol. 6, 1998, pp. 3337–3340.

Semideﬁnite optimization,” in Proc. 37th Annu. Allerton Conf. Com-

[6] M. Schubert and H. Boche, “Solution of the multiuser downlink beam-

mun., Control, and Comput., 1999, pp. 987 – 996.

forming problem with individual SINR constraints,” IEEE Trans. Veh.

[21] Z. Peng, Z. Chen, C. Pan, G. Zhou, and H. Ren, “Robust transmission

Technol., vol. 53, no. 1, pp. 18–28, Jan. 2004.

design for RIS-aided communications with both transceiver hardware

[7] D. Hammarwall, M. Bengtsson, and B. Ottersten, “On downlink beam-

impairments and imperfect CSI,” IEEE Wireless Commun. Lett., vol. 11,

forming with indeﬁnite shaping constraints,” IEEE Trans. Signal Pro-

no. 3, pp. 528–532, Mar. 2022.

cess., vol. 54, no. 9, pp. 3566–3580, Sep. 2006.

[22] S. Gong, C. Xing, P. Yue, L. Zhao, and T. Q. S. Quek, “Hybrid analog

[8] E. Bj¨ornson, M. Bengtsson, and B. Ottersten, “Optimal multiuser trans-

and digital beamforming for RIS-assisted mmWave communications,”

mit beamforming: A diﬃcult problem with a simple solution structure

IEEE Trans. Wireless Commun., vol. 22, no. 3, pp. 1537–1554, Mar.

[lecture notes],” IEEE Signal Process. Mag., vol. 31, no. 4, pp. 142–148,

2023.

Jul. 2014.

[23] Q. Wu and R. Zhang, “Weighted sum power maximization for intelligent

[9] G. Zheng, K.-K. Wong, and T.-S. Ng, “Throughput maximization in

reﬂecting surface aided SWIPT,” IEEE Wireless Commun. Letters, vol. 9,

linear multiuser MIMO–OFDM downlink systems,” IEEE Trans. Veh.

no. 5, pp. 586–590, May 2020.

Technol., vol. 57, no. 3, pp. 1993–1998, May 2008.

[24] X.-S. Yang, Nature-Inspired Metaheuristic Algorithms. Luniver Press,

[10] R. Bhagavatula and R. W. Heath, “Adaptive limited feedback for sum-

2008.

rate maximizing beamforming in cooperative multicell systems,” IEEE

[25] S. Boyd and L. Vandenberghe, Convex Optimization.

Cambridge

Trans. Signal Process., vol. 59, no. 2, pp. 800–811, Feb. 2011.

University Press, 2004.

IEEE TRANSACTIONS ON WIRELESS COMMUNICATIONS, DOI: 10.1109/TWC.2023.3328713

16

[26] X.-S. Yang, Engineering optimisation: an introduction with metaheuris-

constrained optimization for IRS-Aided MISO communication systems,”

tic applications. Wiley, 2009.

IEEE Wireless Commun. Lett., vol. 10, no. 1, pp. 1–5, Jan. 2021.

[27] ——, “Chapter 13: Fireﬂy algorithm: Variants and applications,” in

[43] H. Yu, H. D. Tuan, A. A. Nasir, T. Q. Duong, and H. V. Poor, “Joint

Swarm Intelligence Algorithms. CRC Press, 2020, pp. 175–186.

design of reconﬁgurable intelligent surfaces and transmit beamforming

[28] T. Yamanaka and K. Higuchi, “Transmitter beamforming control based

under proper and improper gaussian signaling,” IEEE J. Sel. Areas

on ﬁreﬂy algorithm for massive MIMO systems with per-antenna power

Commun., vol. 38, no. 11, pp. 2589–2603, Nov. 2020.

constraint,” in 2017 23rd Asia-Paciﬁc Conf. Commun. (APCC), 2017,

[44] N. S. Perovi´c, L.-N. Tran, M. D. Renzo, and M. F. Flanagan, “Optimiza-

pp. 1–6.

[29] T. A. Le and X.-S. Yang, “Fireﬂy algorithm for beamforming design in

RIS-aided communications systems,” in Proc. IEEE Veh. Techno. Conf.

(VTC 2023-Spring), Jun. 2023, pp. 1–5.

[30] I. Fister, I. F. Jr., X.-S. Yang, and J. Brest, “A comprehensive review of

ﬁreﬂy algorithms,” Swarm and Evolutionary Computation, vol. 13, pp.

34–46, Dec. 2013.

tion of RIS-aided MIMO systems via the cutoﬀ rate,” IEEE Wireless

Communications Letters, vol. 10, no. 8, pp. 1692–1696, Aug. 2021.

[31] X.-S. Yang and X.-S. He, Why the Fireﬂy Algorithm Works? Cham:

Springer International Publishing, 2018, pp. 245–259.

Tuan Anh Le (S’10-M’13-SM’19)

received the

Ph.D. degree in telecommunications research from

[32] W. Windarto and E. Eridani, “Comparison of particle swarm optimiza-

King’s College London, The University of London,

tion and ﬁreﬂy algorithm in parameter estimation of Lotka-Volterra,”

U.K., in 2012. He was a Post-Doctoral Research

AIP Conference Proceedings, vol. 2268, no. 1, p. 050008, 09 2020.

Fellow with the School of Electronic and Electri-

[33] R. Ezzeldin, M. Zelenakova, H. F. Abd-Elhamid, K. Pietrucha-Urbanik,

and S. Elabd, “Hybrid optimization algorithms of ﬁreﬂy with ga and pso

cal Engineering, University of Leeds, Leeds, U.K.

He is a Senior Lecturer at Middlesex University,

for the optimal design of water distribution networks,” Water, vol. 15,

London, U.K. His current research interests include integrated sensing and

no. 10, 2023.

communication (ISAC), RIS-aided communication, RF energy harvesting and

[34] M. Clerc and J. Kennedy, “The particle swarm - explosion, stability,

wireless power transfer, physical-layer security, nature-inspired optimization,

and convergence in a multidimensional complex space,” IEEE Trans.

and applied machine learning for wireless communications. He severed as a

Evolutionary Computation, vol. 6, no. 1, pp. 58–73, 2002.

Technical Program Chair for 26th International Conference on Telecommuni-

[35] D. Bertsimas and J. Tsitsiklis, “Simulated annealing,” Statistical Science,

cations (ICT 2019). He was an Exemplary Reviewer of IEEE Communications

vol. 8, no. 1, pp. 10–15, 1993.

Letters in 2019.

[36] X.-S. Yang, Cuckoo Search and Fireﬂy Algorithm: Theory and Applica-

tions. Studies in Computational Intelligence, 2014.

[37] E. Osaba, X.-S. Yang, F. Diaz, E. Onieva, A. D. Masegosa, and A. Peral-

los, “A discrete ﬁreﬂy algorithm to solve a rich vehicle routing problem

modelling a newspaper distribution system with recycling policy,” Soft

Computing, vol. 21, pp. 5295–5308, 2017.

[38] H. Dahrouj and W. Yu, “Coordinated beamforming for the multicell

multi-antenna wireless system,” IEEE Trans. Wireless Commun., vol. 9,

no. 5, pp. 1748–1759, May 2010.

[39] T. A. Le and M. R. Nakhai, “Downlink optimization with interference

pricing and statistical CSI,” IEEE Trans. Commun., vol. 61, no. 6, pp.

2339–2349, Jun 2013.

[40] CVX Research Inc., “CVX: Matlab software for disciplined convex

programming, academic users,” http://cvxr.com/cvx, 2015.

[41] K.-Y. Wang, A. M.-C. So, T.-H. Chang, W.-K. Ma, and C.-Y. Chi,

“Outage constrained robust transmit optimization for multiuser MISO

downlinks: Tractable approximations by conic optimization,” IEEE

Trans. Signal Process., vol. 62, no. 21, pp. 5690–5705, Nov. 2014.

[42] T. A. Le, T. V. Chien, and M. D. Renzo, “Robust probabilistic-

Xin-She Yang obtained his DPhil in Applied Math-

ematics from the University of Oxford. He then

worked at Cambridge University and National Phys-

ical Laboratory (UK) as a Senior Research Scientist.

Now he is Reader at Middlesex University London,

and a co-Editor of the Springer Tracts in Nature-

Inspired Computing. He is also an elected Fellow

of the Institute of Mathematics and its Applications. He was the IEEE

Computational Intelligence Society (CIS) chair for the Task Force on Business

Intelligence and Knowledge Management (2015 to 2020). He has published

more than 300 peer-reviewed research papers with more than 84,000 citations,

and he has been on the prestigious list of highly-cited researchers (Web of

Sciences) for eight consecutive years (2016-2023).

