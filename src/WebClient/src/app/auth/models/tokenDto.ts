export interface TokenWrapper {
  accessToken: TokenDto;
  refreshToken: TokenDto;
}

export interface TokenDto {
  token: string;
  expiresAt: Date;
}