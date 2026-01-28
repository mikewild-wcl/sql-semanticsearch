--select * from documents order by Created desc

/*
--Backup summary embeddings
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = object_id(N'dbo.DocumentSummaryEmbeddingsBackup') AND type IN (N'U'))
	DROP TABLE dbo.DocumentSummaryEmbeddingsBackup;

CREATE TABLE dbo.DocumentSummaryEmbeddingsBackup (
	Id INT PRIMARY KEY,
	Embedding VECTOR(768),
	Created DATETIME2
)

INSERT INTO dbo.DocumentSummaryEmbeddingsBackup (Id, Embedding, Created)
	SELECT Id, Embedding, Created FROM DocumentSummaryEmbeddings 

SELECT * FROM dbo.DocumentSummaryEmbeddingsBackup
*/

declare @query nvarchar(max) = 'Find papers on neutrinos'
declare @k int = 5;
DECLARE @vector VECTOR(768);
               
SELECT @vector = AI_GENERATE_EMBEDDINGS(@query USE MODEL SemanticSearchEmbeddingModel);

--Summary query
SELECT TOP(@k)  VECTOR_DISTANCE('cosine', ds.embedding, @vector) AS [Distance],
                [ArxivId],
                [Title],
                [Summary],
                [Comments],
                [Metadata],
                [PdfUri],
                [Published]               
FROM dbo.Documents d
INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id
ORDER BY [Distance] ASC;

/*
UPDATE dbo.DocumentSummaryEmbeddings 
SET [Embedding] = AI_GENERATE_EMBEDDINGS(FORMATMESSAGE('Title: %s. Summary: %s', d.[Title], d.[Summary]) USE MODEL SemanticSearchEmbeddingModel)
--SELECT
--FORMATMESSAGE('Title: %s. Summary: %s', d.[Title], d.[Summary])
FROM dbo.Documents d
    INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id
*/

/* With title + summary embeddings:
0.344777166843414	1110.2832v2	Can apparent superluminal neutrino speeds be explained as a quantum weak measurement?	Probably not.
0.394673526287079	2412.12121v1	NLLG Quarterly arXiv Report 09/24: What are the most influential current AI Papers?	The NLLG (Natural Language Learning & Generation) arXiv reports assist in navigating the rapidly evolving landscape of NLP and AI research across cs.CL, cs.CV, cs.AI, and cs.LG categories. This fourth installment captures a transformative period in AI history - from January 1, 2023, following ChatGPT's debut, through September 30, 2024. Our analysis reveals substantial new developments in the field - with 45% of the top 40 most-cited papers being new entries since our last report eight months ago and offers insights into emerging trends and major breakthroughs, such as novel multimodal architectures, including diffusion and state space models. Natural Language Processing (NLP; cs.CL) remains the dominant main category in the list of our top-40 papers but its dominance is on the decline in favor of Computer vision (cs.CV) and general machine learning (cs.LG). This report also presents novel findings on the integration of generative AI in academic writing, documenting its increasing adoption since 2022 while revealing an intriguing pattern: top-cited papers show notably fewer markers of AI-generated content compared to random samples. Furthermore, we track the evolution of AI-associated language, identifying declining trends in previously common indicators such as "delve".
0.420567452907562	1410.5401v2	Neural Turing Machines	We extend the capabilities of neural networks by coupling them to external memory resources, which they can interact with by attentional processes. The combined system is analogous to a Turing Machine or Von Neumann architecture but is differentiable end-to-end, allowing it to be efficiently trained with gradient descent. Preliminary results demonstrate that Neural Turing Machines can infer simple algorithms such as copying, sorting, and associative recall from input and output examples.
0.43197363615036	1704.01212v2	Neural Message Passing for Quantum Chemistry	Supervised learning on molecules has incredible potential to be useful in chemistry, drug discovery, and materials science. Luckily, several promising and closely related neural network models invariant to molecular symmetries have already been described in the literature. These models learn a message passing algorithm and aggregation procedure to compute a function of their entire input graph. At this point, the next step is to find a particularly effective variant of this general approach and apply it to chemical prediction benchmarks until we either solve them or reach the limits of the approach. In this paper, we reformulate existing models into a single common framework we call Message Passing Neural Networks (MPNNs) and explore additional novel variations within this framework. Using MPNNs we demonstrate state of the art results on an important molecular property prediction benchmark; these results are strong enough that we believe future work should focus on datasets with larger molecules or more accurate ground truth labels.
0.444075226783752	physics/0702069v2	Would Bohr be born if Bohm were born before Born?	I discuss a hypothetical historical context in which a Bohm-like deterministic interpretation of the Schrodinger equation could have been proposed before the Born probabilistic interpretation and argue that in such a context the Copenhagen (Bohr) interpretation would probably have never achieved great popularity among physicists.
*/

/* To add title embeddings table */
/*
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = object_id(N'dbo.DocumentTitleEmbeddings') AND type IN (N'U'))
    DROP TABLE dbo.DocumentTitleEmbeddings;

CREATE TABLE dbo.DocumentTitleEmbeddings (
    [Id] INT NOT NULL CONSTRAINT [PK_DocumentTitleEmbeddings] PRIMARY KEY,
    [Embedding] VECTOR(768) NOT NULL,
    [Created] DATETIME2(7) NOT NULL CONSTRAINT DF_DocumentTitleEmbeddings_Created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_DocumentTitleEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id))
*/

/* Reset summary embeddings and update or insert title embeddings */
/*
UPDATE dbo.DocumentSummaryEmbeddings 
SET [Embedding] = AI_GENERATE_EMBEDDINGS(d.[Summary] USE MODEL SemanticSearchEmbeddingModel)
FROM dbo.Documents d
    INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id

UPDATE dbo.DocumentTitleEmbeddings 
SET [Embedding] = AI_GENERATE_EMBEDDINGS(d.[Title] USE MODEL SemanticSearchEmbeddingModel)
FROM dbo.Documents d
    INNER JOIN dbo.DocumentTitleEmbeddings ds ON ds.id = d.id

INSERT INTO dbo.DocumentTitleEmbeddings ([Id], [Embedding])
SELECT Id,
       AI_GENERATE_EMBEDDINGS(d.[Title] USE MODEL SemanticSearchEmbeddingModel)
FROM dbo.Documents d

SELECT * FROM DocumentTitleEmbeddings 

*/

SELECT TOP(@k)  VECTOR_DISTANCE('cosine', dt.embedding, @vector) AS [Distance],
                [ArxivId],
                [Title],
                [Summary],
                [Comments],
                [Metadata],
                [PdfUri],
                [Published]               
FROM dbo.Documents d
INNER JOIN dbo.DocumentTitleEmbeddings dt ON dt.id = d.id
ORDER BY [Distance] ASC;

--Combined query            
SELECT TOP(@k)  
    LEAST(
        VECTOR_DISTANCE('cosine', ds.embedding, @vector),
        VECTOR_DISTANCE('cosine', dt.embedding, @vector)
    ) as Distance,
    [ArxivId],
    [Title],
    [Summary],
    [Comments],
    [Metadata],
    [PdfUri],
    [Published]               
FROM dbo.Documents d
INNER JOIN dbo.DocumentTitleEmbeddings dt ON dt.id = d.id
INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id
ORDER BY [Distance] ASC;