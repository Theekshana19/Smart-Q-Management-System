import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <div class="app-shell">
      <router-outlet />
      <p class="smartq-watermark" aria-hidden="true">SmartQ Sri Lanka</p>
    </div>
  `,
  styleUrl: './app.scss'
})
export class App {}
