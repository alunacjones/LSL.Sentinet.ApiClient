const pathActionHandlers = {
    copy: (pathAction, node) => {
        node.responses[pathAction.copyTo] = node.responses[pathAction.copyFrom]
    },
    create: (pathAction, node) => {
        node.responses[pathAction.response] = { 
            description: pathAction.description ?? pathAction.response,
            content: pathAction.responseTypes.reduce((agg, rt) =>
            {
                agg[rt] = pathAction.value;
                return agg;
            },
            {})
        }
    }
}

function runPathAction(pathAction, node) {
    const runner = pathActionHandlers[pathAction.type];
    if (!runner) throw `Unknown path action of ${pathAction.type}`

    runner(pathAction, node);
}

exports.runPathAction = runPathAction;