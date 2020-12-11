// Copyright 2021 The Nakama Authors & Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

interface AddNumbersRequest {
    A : number
    B : number
}

class AddNumbersResponse {
    Result : number;

    constructor(result: number) {
        this.Result = result;
    }
}

const AddNumbers: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, 
            logger: nkruntime.Logger, 
            nk: nkruntime.Nakama, 
            payload: string): string {
    
    let request: AddNumbersRequest = JSON.parse(payload);

    let addNumbersResponse = 
        new AddNumbersResponse( request.A + request.B); 

    logger.debug('example_03.ts, AddNumbers (%s, %s) = %s', 
        request.A, request.B, addNumbersResponse.Result);

    return JSON.stringify(addNumbersResponse);
}
