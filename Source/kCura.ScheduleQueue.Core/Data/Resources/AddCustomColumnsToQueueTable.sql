/****** 
Script Date:		7/25/2016
Script Creator:		Sorawit Amornborvornwong
Script Description:	Adding stop state column onto the schedule queue table.
******/

USE [EDDS]

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[eddsdbo].[{0}]') AND type in (N'U'))
BEGIN
	RAISERROR('{0} does not exist', 18, 1)
END

IF NOT EXISTS(
    SELECT *
    FROM sys.columns 
    WHERE Name      = N'StopState'
      AND Object_ID = Object_ID(N'[eddsdbo].[{0}]'))
BEGIN
    ALTER TABLE [eddsdbo].[{0}]
	ADD StopState int NOT NULL DEFAULT 0
END

IF NOT EXISTS(
    SELECT *
    FROM sys.columns 
    WHERE Name      = N'Heartbeat'
      AND Object_ID = Object_ID(N'[eddsdbo].[{0}]'))
BEGIN
    ALTER TABLE [eddsdbo].[{0}]
	ADD Heartbeat [datetime]
END