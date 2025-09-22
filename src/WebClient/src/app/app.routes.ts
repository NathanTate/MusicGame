import { Routes } from '@angular/router';
import { MainComponent } from './core/main/main.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { HomeComponent } from './features/home/home.component';
import { authGuard } from './auth/guards/auth.guard';
import { canAuthGuard } from './auth/guards/canAuth.guard';
import { PlaylistComponent } from './features/playlist/playlist.component';
import { songsResolver } from './core/resolvers/songs.resolver';
import { NotFoundComponent } from './core/components/not-found/not-found.component';
import { playlistResolver } from './core/resolvers/playlist.resolver';
import { UploadTrackComponent } from './features/upload-track/upload-track.component';
import { genresResolver } from './core/resolvers/genres.resolver';
import { SearchPageComponent } from './features/search-page/search-page.component';
import { TrackPageComponent } from './features/track-page/track-page.component';
import { songResolver } from './core/resolvers/song.resolver';
import { ShowAllComponent } from './features/show-all/show-all.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [canAuthGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [canAuthGuard] },
  {
    path: '',
    component: MainComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        resolve: {
          songs: songsResolver
        },
        component: HomeComponent
      },
      {
        path: 'playlist/:id',
        runGuardsAndResolvers: 'always',
        resolve: {
          playlist: playlistResolver
        },
        component: PlaylistComponent
      },
      {
        path: 'track/:id',
        runGuardsAndResolvers: 'always',
        resolve: {
          song: songResolver
        },
        component: TrackPageComponent
      },
      {
        path: 'upload-track',
        resolve: {
          genres: genresResolver
        },
        component: UploadTrackComponent
      },
      {
        path: 'admin',
        loadChildren: () => import('./features/admin/admin.routes')
          .then(m => m.routes)
      },
      {
        path: 'search/:searchTerm',
        component: SearchPageComponent
      },
      {
        path: 'show-all',
        component: ShowAllComponent
      },
    ]
  },
  { path: '**', component: NotFoundComponent }
];
