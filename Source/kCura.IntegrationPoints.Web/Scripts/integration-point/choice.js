var Choice = function (name, value, artifactID, object, belongsToApplication) {
	this.displayName = name;
	this.value = value;
	this.artifactID = artifactID;
	this.model = object;
	this.belongsToApplication = belongsToApplication;
};