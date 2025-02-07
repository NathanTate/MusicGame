import { inject, Injectable, signal } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { HttpClient } from "@angular/common/http";
import { PlaylistListResponse } from "../models/playlist/playlistListResponse";
import { SongListResponse } from "../models/song/songListResponse";
import { UserPlaylistsQuery } from "../models/queries/userPlaylistsQuery";
import { UserSongsQuery } from "../models/queries/userSongsQuery";
import { generateHttpParams } from "../../shared/helpers/httpParamsHelper";
import { PlaylistResponse, PlaylistSongResponse } from "../models/playlist/playlistResponse";
import { ArtistResponse } from "../models/user/artistResponse";

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly _baseUrl = environment.apiUrl + 'users/';
  public readonly playlistsQuery = new UserPlaylistsQuery();
  public readonly songsQuery = new UserSongsQuery();
  private readonly http = inject(HttpClient);

  public likedSongsPlaylist = signal<PlaylistResponse | null>(null);

  public getUserPlaylists(userId: string, query: UserPlaylistsQuery) {
    const httpParams = generateHttpParams(query);

    return this.http.get<PlaylistListResponse>(this._baseUrl + `${userId}/playlists`, { params: httpParams });
  }

  public getUserLikedPlaylists(userId: string, query: UserPlaylistsQuery) {
    const httpParams = generateHttpParams(query);

    return this.http.get<PlaylistListResponse>(this._baseUrl + `${userId}/playlists/liked`, { params: httpParams });
  }

  public getUserSongs(userId: string, query: UserSongsQuery) {
    const httpParams = generateHttpParams(query);

    return this.http.get<SongListResponse>(this._baseUrl + `${userId}/songs`, { params: httpParams });
  }

  public getUserLikedSongs(userId: string, query: UserSongsQuery) {
    const httpParams = generateHttpParams(query);

    return this.http.get<SongListResponse>(this._baseUrl + `${userId}/songs/liked`, { params: httpParams });
  }

  public createPlaylistFromSongs(songs: SongListResponse, user: ArtistResponse) {
    let playlistSongs: PlaylistSongResponse[] = [];
    let totalDuration = 0;
    songs.items.forEach((item, index) => {
      totalDuration += item.duration;
      const playlistSong: PlaylistSongResponse = {
        position: index,
        song: item
      }
      playlistSongs.push(playlistSong);
    })

    const playlist: PlaylistResponse = {
      playlistId: -1,
      name: "Liked songs",
      isPrivate: true,
      totalDuration: 0,
      songsCount: songs.totalCount,
      likesCount: 0,
      createdAt: "",
      songs: playlistSongs,
      photoUrl: 'https://misc.scdn.co/liked-songs/liked-songs-300.jpg',
      user: user
    }

    return playlist;
  }
}