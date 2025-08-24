import {UserDto} from '../../modules/lpshows-api';

export class User {
  static fromClaims(claims: any): UserDto {
    let user: UserDto = {};
    user.id = claims['sub'];
    user.username = claims['custom:display_name'];
    user.email = claims['email'];
    user.emailVerified = claims['email_verified'];

    return user;
  }
}
