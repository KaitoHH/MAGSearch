# MAGSearch
**The problem description is copied from [here](https://studentclub.msra.cn/bop2016/topic/), from beauty of programming 2016, by Microsoft**.

## Description
Microsoft Academic Graph (MAG) is a large heterogeneous graph containing entities such as authors, papers, journals, conferences and relations between them. Microsoft provides Academic Knowledge API for this contest. The Entity attributes are defined here.
Participants are supposed to provide a REST service endpoint that can find all the 1-hop, 2-hop, and 3-hop graph paths connecting a given pair of entity identifiers in MAG. The given pair of entity identifiers could be [Id, Id], [Id, AA.AuId], [AA.AuId, Id], [AA.AuId, AA.AuId]. Each node of a path should be one of the following identifiers: Id, F.Fid, J.JId, C.CId, AA.AuId, AA.AfId. Possible edges (a pair of adjacent nodes) of a path are:
![topic](https://studentclub.msra.cn/static/images/bop/2016/topic.png)

For each test case, the REST service endpoint will receive a JSON array via HTTP with a pair of entity identifiers, where the identifiers are 64-bit integers, e.g. [123, 456]. The service endpoint needs to respond with a JSON array within 300 seconds. The response JSON array consists of a list of graph paths in the form of [path1, path2, …, pathn], where each path is an array of entity identifiers. For example, if your program finds one 1-hop paths, two 2-hop paths, and one 3-hop paths, the results may look like this: [[123,456], [123,2,456], [123,3,456], [123,4,5,456]]. For a path such as [123,4,5,456], the integers are the identifiers of the entities on the path. After receiving the response, the evaluator will wait for a random period of time before sending the next requests.

## Evaluation Metric
The REST service must be deployed to a Standard_A3 virtual machine for the final test. There are no constraints on the programming language you can use.
The test cases are not available before the final evaluation. When the evaluation starts, the evaluator system sends test cases to the REST endpoint of each team individually. Each team will receive 10 test cases (Q1to Q10). The response time for test case Qi is recorded as Ti(1≤i≤10). The final score is calculated using:
![score](https://studentclub.msra.cn/static/images/bop/2016/score.png)

where Ni is the size of the solution (the total number of correct paths) for Qi , Ki is the total number of paths returned by the REST service, Mi is the number of distinct correct paths returned by the REST service.

## Our result
- We had a really hard time working on this problem, and finally our team got 74 of nearly 250 teams. ehm.. it's a not good and not bad solutions, we find all the paths, but stil get an unsatisfactory due to the poor effencicy.
- However we stil decide to share all the code for learning and communicating :)
