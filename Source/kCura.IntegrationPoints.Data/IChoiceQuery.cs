﻿using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IChoiceQuery
	{
		List<Choice> GetChoicesOnField(int fieldArtifactId);
		List<Choice> GetChoicesOnField(Guid fieldGuid);
	}
}