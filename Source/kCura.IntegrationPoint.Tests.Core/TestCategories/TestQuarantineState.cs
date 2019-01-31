﻿namespace kCura.IntegrationPoint.Tests.Core.TestCategories
{
    /// <summary>
    /// The main purpose of these states is to help in monitoring of 
    /// unstable tests placed in the quarantine. A test should be marked
    /// with a specific state if it meets following conditions.
    /// </summary>
	public enum TestQuarantineState
	{
        /// <summary>
        /// Initial state of a test in the quarantine. 
        /// A test should stay with it no longer than duration of a single 
        /// sprint (two weeks). After two weeks, we have enough data to 
        /// decide what action should be made to each test in the quarantine.
        /// </summary>
        UnderObservation,
        /// <summary>
        /// Particular test still fails because of the defect
        /// in a product and should be fixed as soon as possible. After fixing, 
        /// it should be marked with SeemsToBeStable.
        /// </summary>
        DetectsDefect,
        /// <summary>
        /// A test randomly fails because of some instability
        /// issues in itself. The reason of instability should be described.
        /// After fixing, it should be marked with SeemsToBeStable.
        /// </summary>
        ShowsInstability,
        /// <summary>
        /// A test still succeeds and looks like stable.
        /// On the next round of reviewing the quarantine, it should be moved
        /// back to stable tests if reviewer does not find any failure in a history.
        /// </summary>
        SeemsToBeStable
    }
}
