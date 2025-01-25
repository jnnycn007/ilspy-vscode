// Copyright (c) 2023 ICSharpCode
// Licensed under the MIT license. See the LICENSE file in the project root for more information.


using ILSpyX.Backend.Application;
using ILSpyX.Backend.LSP.Protocol;
using ILSpyX.Backend.Model;
using OmniSharp.Extensions.JsonRpc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ILSpyX.Backend.LSP.Handlers;

[Serial, Method("ilspy/getNodes", Direction.ClientToServer)]
public class GetNodesHandler(ILSpyXApplication application) : IJsonRpcRequestHandler<GetNodesRequest, GetNodesResponse>
{
    public async Task<GetNodesResponse> Handle(GetNodesRequest request, CancellationToken cancellationToken)
    {
        (var nodes, bool shouldUpdateAssemblyList) =
            await application.DecompilerBackend.DetectAutoLoadedAssemblies(() =>
                application.TreeNodeProviders.ForNode(request.NodeMetadata)
                    .GetChildrenAsync(request.NodeMetadata));
        return new GetNodesResponse(nodes, shouldUpdateAssemblyList);
    }
}
