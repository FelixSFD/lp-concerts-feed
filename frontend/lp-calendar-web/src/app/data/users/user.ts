export class User {
  id: string | undefined;
  username: string | undefined;
  email: string | undefined;
  emailVerified: boolean = false;


  static fromClaims(claims: any): User {
    let user =  new User();
    user.id = claims['sub'];
    user.username = claims['custom:display_name'];
    user.email = claims['email'];
    user.emailVerified = claims['email_verified'];

    return user;
  }
}
