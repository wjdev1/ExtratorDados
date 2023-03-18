create or alter procedure sp_RAIS_ObterRelatorio
AS
BEGIN
WITH DADOS AS(
SELECT distinct
YEAR(rem.DataPagamento) as Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,rais.DataAdmissao,103) AS DataAdmissao,
isnull(CONVERT(varchar,DataDesligamento,103),'	') AS DataDesligamento,
isnull(CONVERT(varchar(3),CodCausaDesligamento),' ') AS CodCausaDeslig,
CONVERT(varchar(10),MONTH(rem.DataPagamento)) + '/' + CONVERT(varchar(10),YEAR(rem.DataPagamento)) 'MesAno',
'CompNaoLocalizada' = 
CASE WHEN 1 = (SELECT DISTINCT '1' FROM RAIS..CompNaoLocalizada WHERE Ano = YEAR(rem.DataPagamento) 
AND MesAno = REM.DataPagamento AND PIS = RAIS.PIS) 
THEN '1' 
when 1 = (SELECT DISTINCT 1 FROM RAIS..CompNaoLocalizada WHERE PIS = RAIS.PIS AND Informacao = 'SEM OCORRENCIAS' AND RAIS.DataAdmissao = DataAdmissao)
THEN 'Sem Ocorrências'
ELSE '	' END,
'MesesNaoDepositados' = ISNULL((SELECT distinct '1' 
FROM RAIS..ColaboradorRAIS r 
WHERE r.CPF = rais.CPF and r.Ano = rais.Ano and 
exists(SELECT 1 FROM RAIS..ExtratoFGTS e WHERE E.CPF = RAIS.CPF AND E.Data = REM.DataPagamento)),''),
rem.ValorPagamento 'Remuneracao',
REM.DataPagamento,
MONTH(REM.DataPagamento) 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..Remuneracao rem on rem.CPFColaborador = rais.CPF
AND (YEAR(rem.DataPagamento) <= YEAR(rais.DataDesligamento)
AND MONTH(rem.DataPagamento) <= MONTH(rais.DataDesligamento)
OR rais.DataDesligamento is null)
AND rem.DataPagamento >= '2011-09-01'
AND rem.DataPagamento <= '2016-09-30'
WHERE rem.ValorPagamento > 0 AND rais.Ano = YEAR(rem.DataPagamento) and rais.DataAdmissao = rem.DataAdmissao
UNION --13º adiantamento
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,DECI.DataAdmissao,103) AS DataAdmissao,
isnull(CONVERT(varchar,DECI.DataDesligamento,103),'') AS DataDesligamento,
isnull(CONVERT(varchar(3),DECI.CodCausaDesligamento),''),
'13º adiantamento',
'  ',
'   ',
DECI.DecTerceiroAdiantamento 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
14 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
AND DECi.DataAdmissao = rais.DataAdmissao
WHERE DECI.DecTerceiroAdiantamento > 0 AND DECI.Ano < 2016 AND (DECI.DataDesligamento >= '2011-09-01' OR DECI.DataDesligamento is null)
UNION --13º adiantamento regra demissão após setembro 2016
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,DECI.DataAdmissao,103) AS DataAdmissao,
isnull(CONVERT(varchar,DECI.DataDesligamento,103),'') AS DataDesligamento,
isnull(CONVERT(varchar(3),DECI.CodCausaDesligamento),''),
'13º adiantamento',
'  ',
'   ',
DECI.DecTerceiroAdiantamento 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
14 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
AND DECi.DataAdmissao = rais.DataAdmissao
WHERE DECI.DecTerceiroAdiantamento > 0 AND DECI.Ano = 2016 AND rais.DataDesligamento < '2016-10-01'
UNION --13º parcela final
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,DECI.DataAdmissao,103) AS DataAdmissao,
isnull(CONVERT(varchar,DECI.DataDesligamento,103),'') AS DataDesligamento,
isnull(CONVERT(varchar(3),DECI.CodCausaDesligamento),''), 
'13º parcela final',
' ',
'  ',
DECI.DecTerceiroParcFinal 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
15 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
AND DECi.DataAdmissao = rais.DataAdmissao
WHERE DECI.DecTerceiroParcFinal > 0 AND DECI.ANO < 2016 AND (DECI.DataDesligamento >= '2011-09-01' OR DECI.DataDesligamento is null)
UNION --13º parcela final regra demissão após setembro 2016
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,DECI.DataAdmissao,103) AS DataAdmissao,
isnull(CONVERT(varchar,DECI.DataDesligamento,103),'') AS DataDesligamento,
isnull(CONVERT(varchar(3),DECI.CodCausaDesligamento),''), 
'13º parcela final',
' ',
'  ',
DECI.DecTerceiroParcFinal 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
15 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
AND DECi.DataAdmissao = rais.DataAdmissao
WHERE DECI.DecTerceiroParcFinal > 0 AND DECI.ANO = 2016 AND rais.DataDesligamento < '2016-10-01'
UNION --aviso prévio
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,rais.DataAdmissao,103) AS DataAdmissao,
isnull(CONVERT(varchar,rais.DataDesligamento,103),'') AS DataDesligamento,
isnull(CONVERT(varchar(3),rais.CodCausaDesligamento),''),
'Aviso prévio',
'  ',
'  ',
DECI.AvisoPrevio 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
16 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
WHERE DECI.AvisoPrevio > 0 AND ((DECI.DataDesligamento >= '2011-09-01' AND rais.DataDesligamento < '2016-10-01') OR DECI.DataDesligamento is null )
)
SELECT 
*
FROM DADOS D
WHERE NOT exists (SELECT * FROM RAIS..EXCLUSAOCOLABORADOR WHERE trim(Nome) = trim(D.Nome))
ORDER BY Nome,DataPagamento,ordem
END