WITH DADOS AS(
SELECT distinct
YEAR(rem.DataPagamento) as Ano_,
Nome,PIS,rais.CPF,
CONVERT(varchar,DataAdmissao,104) AS DataAdmissao,
CONVERT(varchar,DataDesligamento,104) AS DataDesligamento,
CodCausaDesligamento,
CONVERT(varchar(10),MONTH(rem.DataPagamento)) + '/' + CONVERT(varchar(10),YEAR(rem.DataPagamento)) 'Mes/Ano',
'CompNaoLocalizada' = (SELECT DISTINCT 1 FROM RAIS..CompNaoLocalizada WHERE Ano = YEAR(rem.DataPagamento) 
AND MesAno = REM.DataPagamento AND PIS = RAIS.PIS),
'MesesNaoDepositados' = (SELECT distinct 1 
FROM RAIS..ColaboradorRAIS r 
WHERE r.CPF = rais.CPF and r.Ano = rais.Ano and 
exists(SELECT 1 FROM RAIS..ExtratoFGTS e WHERE E.CPF = RAIS.CPF AND E.Data = REM.DataPagamento)),
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
WHERE rem.ValorPagamento > 0 AND rais.Ano = YEAR(rem.DataPagamento)
UNION --13º adiantamento
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,DECI.DataAdmissao,104) AS DataAdmissao,
CONVERT(varchar,DECI.DataDesligamento,104) AS DataDesligamento,
DECI.CodCausaDesligamento,
'',
'',
'',
DECI.DecTerceiroAdiantamento 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
14 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
AND DECi.DataAdmissao = rais.DataAdmissao
WHERE DECI.DecTerceiroAdiantamento > 0
--group by Deci.Ano,RAIS.Nome,RAIS.PIS,RAIS.CPF,rais.DataDesligamento
UNION --13º parcela final
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,DECI.DataAdmissao,104) AS DataAdmissao,
CONVERT(varchar,DECI.DataDesligamento,104) AS DataDesligamento,
DECI.CodCausaDesligamento,
'',
'',
'',
DECI.DecTerceiroParcFinal 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
15 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
AND DECi.DataAdmissao = rais.DataAdmissao
WHERE DECI.DecTerceiroParcFinal > 0
--group by Deci.Ano,RAIS.Nome,RAIS.PIS,RAIS.CPF,rais.DataDesligamento
UNION --aviso prévio
SELECT distinct
deci.Ano,
Nome,PIS,rais.CPF,
CONVERT(varchar,rais.DataAdmissao,104) AS DataAdmissao,
CONVERT(varchar,rais.DataDesligamento,104) AS DataDesligamento,
rais.CodCausaDesligamento,
CONVERT(varchar(2),12) + '/' + CONVERT(varchar(4),deci.Ano) 'Mes/Ano',
'',
'',
DECI.AvisoPrevio 'Remuneracao',
CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',
16 'ordem'
FROM RAIS..ColaboradorRAIS rais
INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF
WHERE DECI.AvisoPrevio > 0
)
SELECT 
*
FROM DADOS 
ORDER BY Nome,DataPagamento,ordem