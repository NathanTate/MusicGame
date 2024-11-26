import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PrimeNGConfig } from 'primeng/api';
import { Button } from 'primeng/button';
import { NavbarComponent } from "./core/components/navbar/navbar.component";
import { MainComponent } from "./core/main/main.component";
import { FooterComponent } from "./core/components/footer/footer.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Button, NavbarComponent, MainComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  primengConfig = inject(PrimeNGConfig);

  ngOnInit(): void {
    this.primengConfig.ripple = true;
  }
}
