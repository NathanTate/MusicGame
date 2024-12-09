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
        path: 'admin',
        loadChildren: () => import('./features/admin/admin.routes')
          .then(m => m.routes)
      }
    ]
  },
  { path: '**', component: NotFoundComponent }
];
