import { Injectable } from '@angular/core';

@Injectable()
export class AuthService {
    private authority: string = 'https://login.windows.net/common';
    private redirectUri: string = 'http://restlessmindsservices.azurewebsites.net';
    private clientId: string = '0f91ee12-f82b-42b6-8c25-ac7140e7dfec';
    private resourceUri: string = 'https://graph.windows.net';
    private accesToken: string = '';
    private error: string = '';
    private expiresOn: string = '';

    public logIn(): Promise<any> {
        return new Promise((resolve, reject) => {
            this.auth(
                (authResponse) => {
                    this.accesToken = authResponse.accesToken;
                    this.expiresOn = authResponse.expiresOn;
                    resolve(authResponse);
                },
                (err) => {
                    this.error = err;
                    reject(err);
                }
            );
        });
    }

    private auth(authCompletedCallback, errorCallback): void {
        var authContext = new Microsoft.ADAL.AuthenticationContext(this.authority);
        authContext.tokenCache.readItems().then((items: any[]) => {
            if (items.length > 0) {
                let authority = items[0].authority;
                authContext = new Microsoft.ADAL.AuthenticationContext(authority);
            }
            // Attempt to authorize user silently
            authContext.acquireTokenSilentAsync(this.resourceUri, this.clientId, '')
                .then(authCompletedCallback, () => {
                    // We require user cridentials so triggers authentication dialog
                    authContext.acquireTokenAsync(this.resourceUri, this.clientId, this.redirectUri)
                        .then(authCompletedCallback, errorCallback);
                });
        });

    };
}
