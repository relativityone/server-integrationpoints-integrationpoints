(function () {
    const { suite, test } = intern.getInterface('tdd');
    const { assert } = intern.getPlugin('chai');

    suite('step-mapFields.js', () => {

        suite('Step', () => {

            const destinationModelJSONMock = JSON.stringify({ artifactTypeID: 7});
            const modelMock = { 
                destination: destinationModelJSONMock,
                IPDestinationSettings: { 
                    IdentifierField: ''
                },
                SourceProviderConfiguration: {
                    importSettingVisibility: ''
                },
                source: {
                    selectedType: ''
                }
            };
            IP.points.steps[0].loadModel(modelMock);

            test('test1', () => {
                console.log('IP.points.steps[0]', IP.points.steps[0]);
            });

        });
    });

})();    