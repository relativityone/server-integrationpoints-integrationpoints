using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.EventHandlers
{
	[Description("Adds a table to store information for the document volume table.")]
	[Guid("67722BDD-3E1C-4C34-8559-68E9412BF79A")]
	[RunOnce(false)]
	public class AddDocumentVolumeTable : PostInstallEventHandler
	{
		public override Response Execute()
		{
			Response response = ExecuteInstanced();
			return response;
		}

		internal Response ExecuteInstanced()
		{
			Response response = new Response() { Success = true, Message = "Upgrade successful." };

			try
			{
				ICaseServiceContext caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
				caseServiceContext.SqlContext.ExecuteNonQuerySQLStatement(_CREATE_DOCUMENT_VOLUME_TABLE_SQL);
				caseServiceContext.SqlContext.ExecuteNonQuerySQLStatement(_CREATE_DOCUMENT_VOLUME_STORED_PROC_SQL);
			}
			catch (System.Exception ex)
			{
				response.Success = false;
				response.Exception = ex;
				response.Message = ex.Message;
			}
			return response;
		}

		#region SQL Queries

		private const string _CREATE_DOCUMENT_VOLUME_TABLE_SQL = @"
			IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
								WHERE TABLE_SCHEMA = 'EDDSDBO' 
								AND TABLE_NAME = 'DocumentVolume'))
			BEGIN
				CREATE TABLE [DocumentVolume]
				(
					[Date] [date] NOT NULL,
					[DocumentsIncluded] [int] NOT NULL,
					[DocumentsExcluded] [int] NOT NULL,
					[DocumentsUntagged] [int] NOT NULL
				)
			END";

		private const string _CREATE_DOCUMENT_VOLUME_STORED_PROC_SQL = @"
			SET ANSI_NULLS ON
			GO

			SET QUOTED_IDENTIFIER ON
			GO

			CREATE PROCEDURE [EDDSDBO].[UpsertDocumentVolume]
				@date DATE,
				@documentsIncluded INT,
				@documentsExcluded INT,
				@documentsUntagged INT
			AS

				DECLARE @currentDate DATE;
				SELECT @currentDate = CAST(@date AS DATE);

				IF EXISTS (SELECT * FROM [DocumentVolume] WHERE [Date] = @currentDate)
					UPDATE [DocumentVolume]
					SET [DocumentsIncluded] = @documentsIncluded,
						[DocumentsExcluded] = @documentsExcluded,
						[DocumentsUntagged] = @documentsUntagged
					WHERE [Date] = @currentDate
				ELSE
					INSERT INTO [DocumentVolume] VALUES (@currentDate, @documentsIncluded, @documentsExcluded, @documentsUntagged)

			GO";

		#endregion
	}
}