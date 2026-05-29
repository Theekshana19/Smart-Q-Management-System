import { Injectable } from '@angular/core';
import { VoiceTemplate } from '../models';

@Injectable({ providedIn: 'root' })
export class VoiceAnnouncementService {
  announceTokenCalled(tokenNo: string, counterName: string, template?: VoiceTemplate | null): void {
    if (!('speechSynthesis' in window)) return;

    let text: string;
    if (template?.templateText) {
      text = template.templateText
        .replace('{tokenNo}', this.spellToken(tokenNo))
        .replace('{counterName}', counterName);
    } else {
      text = `Token number ${this.spellToken(tokenNo)}, please proceed to ${counterName}`;
    }

    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = 'en-US';
    utterance.rate = 0.9;
    window.speechSynthesis.cancel();
    window.speechSynthesis.speak(utterance);
  }

  private spellToken(tokenNo: string): string {
    const [prefix, num] = tokenNo.split('-');
    const spelledPrefix = prefix.split('').join(' ');
    const digits = num?.split('').map(d => (d === '0' ? 'zero' : d)).join(' ') ?? '';
    return `${spelledPrefix} ${digits}`;
  }
}
