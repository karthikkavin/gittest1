This is a change

SELECT t1.* FROM
    (SELECT qalsp.QAInspectionLogSpoolId, '' AS QAInspectionLogWeldId, '' AS WeldDataId, sp.SpoolID, sp.SpoolNo, sp.SpoolRevNo,  '' AS WeldLabel, mimt.MaterialName AS Material, '' AS Size, '' AS WeldDescription,
    sp.RTCompletePercentage, sp.MTCompletePercentage, sp.PTCompletePercentage, sp.PMICompletePercentage, sp.HardnessCompletePercentage, qalsp.Status AS Status, '' AS FitterID ,'' AS WelderID ,'' AS WPS, NULL AS IsQAInspectionWeld, '' AS IsInfo, NULL AS ThirdPartyStatus, sp.PipeCode, qalsp.LogType
    FROM [dbo].[QAInspectionLogSpools] qalsp 
    INNER JOIN (SELECT [SpoolID], [SpoolNo], [SpoolRevNo], [Material], [RTCompletePercentage], [MTCompletePercentage], [PTCompletePercentage], [PMICompletePercentage], [HardnessCompletePercentage], [JobID], [IsDeleted], [PipeCode]
			    FROM [dbo].[Spool] WHERE [JobID] = @jobId AND [SpoolNo] LIKE @spoolNo AND ([IsDeleted] = 0 OR [IsDeleted] IS NULL)
			 )
    sp	ON qalsp.SpoolId = sp.SpoolID
	LEFT JOIN [dbo].[MasterInspections] mis ON qalsp.MasterInspectionId = mis.MasterInspectionId
    LEFT JOIN [dbo].[SpoolReleaseItems] sri ON qalsp.SpoolId = sri.SpoolID    
    LEFT JOIN [dbo].[MasterInspectionMaterials] mimt ON qalsp.MasterInspectionMaterialId = mimt.MaterialId
    WHERE qalsp.[JobId] = @jobId AND mis.MasterInspectionId = @masterinspectionId AND (qalsp.[IsDeleted] = 0 OR qalsp.[IsDeleted] IS NULL)
	AND sri.[IssuedToShop] IS NOT NULL AND (sp.[IsDeleted] = 0 OR sp.[IsDeleted] IS NULL) AND sp.[JobId] = @jobId 
    AND ISNULL(qalsp.Status,0) != @exclude
    and qalsp.spoolid not in (select max(spoolid) spoolid from [dbo].[workonhold] where holdrelease is null group by spoolid)
    and qalsp.spoolid not in (select max(swp.spoolid) spoolid from [dbo].[swospools] swp
                   left join [dbo].[swo] sw on swp.swoid = sw.swoid
                   where (swp.[IsDeleted] = 0 OR swp.[IsDeleted] IS NULL) AND sw.ReleaseDate IS NULL GROUP BY SpoolId)
    UNION
    SELECT '' AS QAInspectionLogSpoolId, qalweld.QAInspectionLogWeldId, qalweld.WeldDataId, sp.SpoolID, sp.SpoolNo, sp.SpoolRevNo, qalweld.WeldLabel, mimet.MaterialName AS Material, acweld.Size_I AS Size,  acweld.WDescript AS WeldDescription, 
	     sp.RTCompletePercentage, sp.MTCompletePercentage,  sp.PTCompletePercentage, sp.PMICompletePercentage, sp.HardnessCompletePercentage, qalweld.Status AS Status, qalweld.FitterID ,qalweld.WelderID ,qalweld.WPS, acweld.IsQAInspectionWeld, qalweld.IsInfo, qashoot.ThirdPartyStatus, sp.PipeCode, qalweld.LogType
     FROM [dbo].[QAInspectionLogWelds] qalweld
    INNER JOIN (SELECT [SpoolID], [SpoolNo], [SpoolRevNo], [Material], [RTCompletePercentage], [MTCompletePercentage], [PTCompletePercentage], [PMICompletePercentage], [HardnessCompletePercentage], [JobID], [IsDeleted],[ShipmentID], [PipeCode]
			    FROM [dbo].[Spool] WHERE [JobID] = @jobId AND [SpoolNo] LIKE @spoolNo AND ([IsDeleted] = 0 OR [IsDeleted] IS NULL)
			 )
    sp	ON qalweld.SpoolId = sp.SpoolID 
	LEFT JOIN [dbo].[MasterInspections] mis ON qalweld.MasterInspectionId = mis.MasterInspectionId
    LEFT JOIN [dbo].[SpoolReleaseItems] sri ON qalweld.SpoolId = sri.SpoolID
    LEFT JOIN [dbo].[WeldData] weld ON qalweld.WeldDataId = weld.Id 
    LEFT JOIN [dbo].[AcornWeld] acweld ON weld.AcornWeldId = acweld.AcornWeldId AND acweld.JobId = @jobId
    LEFT JOIN [dbo].[AspNetUsers] us ON qalweld.UpdatedBy = us.Id
    LEFT JOIN [dbo].[MasterInspectionMaterials] mimet ON qalweld.MasterInspectionMaterialId = mimet.MaterialId
    LEFT JOIN [dbo].[QARandomSelectionQueue] qashoot ON qalweld.QAInspectionLogWeldId = qashoot.QAInspectionLogWeldId
    WHERE qalweld.[JobId] = @jobId AND mis.MasterInspectionId = @masterinspectionId AND (qalweld.[IsDeleted] = 0 OR qalweld.[IsDeleted] IS NULL)
	AND sri.[IssuedToShop] IS NOT NULL AND (sp.[IsDeleted] = 0 OR sp.[IsDeleted] IS NULL) AND sp.[JobId] = @jobId 
    AND ISNULL(qalweld.Status,0) !=  @exclude
    AND qalweld.SpoolId NOT IN (Select MAX(SpoolID) SpoolID FROM [dbo].[WorkOnHold] Where HoldRelease IS NULL GROUP BY SpoolID)
    AND qalweld.SpoolId NOT IN (Select MAX(swp.SpoolID) SpoolID FROM [dbo].[SWOSpools] swp
                        LEFT JOIN [dbo].[SWO] sw ON swp.SWOID = sw.SWOID
                        Where (swp.[IsDeleted] = 0 OR swp.[IsDeleted] IS NULL) AND sw.ReleaseDate IS NULL GROUP BY SpoolId)
    AND weld.JobId = @jobId AND (qashoot.[IsDeleted] = 0 OR qashoot.[IsDeleted] IS NULL)) AS t1
    WHERE 1=1 ORDER BY t1.SpoolNo, t1.SpoolRevNo,  t1.WeldLabel
