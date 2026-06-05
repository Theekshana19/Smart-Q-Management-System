import { Injectable } from '@angular/core';

export type KioskLang = 'EN' | 'SI' | 'TA';

export interface KioskLabels {
  selectServiceTitle: string;
  selectServiceSub: string;
  liveStatus: string;
  waitTimes: string;
  waitTimesDesc: string;
  staffActive: string;
  back: string;
  home: string;
  loading: string;
  loadError: string;
  homeCrumb: string;
  backToMainServices: string;
  selectSubTitle: string;
  selectSubSub: string;
  mins: string;
  customersWaiting: string;
  generatingToken: string;
  systemOnline: string;
  branchIdLabel: string;
  digitalKiosk: string;
  tokenReady: string;
  tokenReadySub: string;
  tokenNumber: string;
  serviceLabel: string;
  estWait: string;
  receiptHold: string;
  printToken: string;
  finish: string;
  counterPending: string;
  scanQr: string;
  smartqBank: string;
  centralDistrict: string;
  branchNum: string;
  tokenNoLabel: string;
  issued: string;
}

const LABELS: Record<KioskLang, KioskLabels> = {
  EN: {
    selectServiceTitle: 'Select Your Service',
    selectServiceSub: 'Please choose a category to receive your queue ticket.',
    liveStatus: 'Live Status',
    waitTimes: 'Wait Times',
    waitTimesDesc: 'Current average wait time is approximately 8 minutes. We value your time.',
    staffActive: '8 Staff Active',
    back: 'Back',
    home: 'Home',
    loading: 'Loading...',
    loadError: 'Failed to load. Please try again.',
    homeCrumb: 'Home',
    backToMainServices: 'Back to Main Services',
    selectSubTitle: 'Select {service}',
    selectSubSub: 'Please choose the specific transaction type to join the queue.',
    mins: 'mins',
    customersWaiting: 'customers waiting',
    generatingToken: 'Generating your token...',
    systemOnline: 'System Online',
    branchIdLabel: 'Bank Branch ID',
    digitalKiosk: 'SmartQ Digital Kiosk',
    tokenReady: 'Your Token is Ready',
    tokenReadySub: 'Please proceed to the waiting area. Your number will be called shortly.',
    tokenNumber: 'TOKEN NUMBER',
    serviceLabel: 'Service',
    estWait: 'Est. Wait',
    receiptHold: 'Please hold on to your receipt. You can track your queue position via the QR code.',
    printToken: 'Print Token',
    finish: 'Finish',
    counterPending: 'Pending Assignment',
    scanQr: 'Scan for Mobile Tracking',
    smartqBank: 'SMARTQ BANK',
    centralDistrict: 'Central Financial District',
    branchNum: 'Branch #0402',
    tokenNoLabel: 'TOKEN NO',
    issued: 'Issued'
  },
  SI: {
    selectServiceTitle: 'ඔබේ සේවාව තෝරන්න',
    selectServiceSub: 'පෝලිම් ටිකට්පත ලබා ගැනීමට කාණ්ඩයක් තෝරන්න.',
    liveStatus: 'සජීවී තත්ත්වය',
    waitTimes: 'පොරොත්තු වේලාව',
    waitTimesDesc: 'සාමාන්‍ය පොරොත්තු වේලාව මිනිත්තු 8 ක් පමණයි. අපි ඔබේ කාලය අගය කරමු.',
    staffActive: 'සක්‍රිය සේවකයින් 8',
    back: 'ආපසු',
    home: 'මුල් පිටුව',
    loading: 'පූරණය වෙමින්...',
    loadError: 'පූරණය කිරීමට අසමත් විය. නැවත උත්සාහ කරන්න.',
    homeCrumb: 'මුල් පිටුව',
    backToMainServices: 'ප්‍රධාන සේවාවලට ආපසු',
    selectSubTitle: '{service} තෝරන්න',
    selectSubSub: 'පෝලිමට එක්වීමට ගනුදෙනු වර්ගය තෝරන්න.',
    mins: 'මිනි',
    customersWaiting: 'පාරිභෝගිකයින් රැඳී සිටී',
    generatingToken: 'ඔබේ ටෝකනය සාදමින්...',
    systemOnline: 'පද්ධතිය සබලයි',
    branchIdLabel: 'ශාඛා හැඳුම',
    digitalKiosk: 'SmartQ ඩිජිටල් කියෝස්ක්',
    tokenReady: 'ඔබේ ටෝකනය සූදානම්',
    tokenReadySub: 'රැඳී සිටින ප්‍රදේශයට යන්න. ඔබේ අංකය ඉක්මනින් කැඳවනු ලැබේ.',
    tokenNumber: 'ටෝකන් අංකය',
    serviceLabel: 'සේවාව',
    estWait: 'ඇස්. පොරොත්තුව',
    receiptHold: 'ඔබේ රිසිට්පත තබා ගන්න. QR කේතයෙන් පෝලිම් තත්ත්වය නිරීක්ෂණය කළ හැක.',
    printToken: 'ටෝකනය මුද්‍රණය',
    finish: 'අවසන්',
    counterPending: 'පවරා නොමැත',
    scanQr: 'ජංගම නිරීක්ෂණය සඳහා ස්කෑන් කරන්න',
    smartqBank: 'ස්මාර්ට්කියු බැංකුව',
    centralDistrict: 'මධ්‍යම මූල්‍ය දිස්ත්‍රික්කය',
    branchNum: 'ශාඛාව #0402',
    tokenNoLabel: 'ටෝකන් අංකය',
    issued: 'නිකුත් කළේ'
  },
  TA: {
    selectServiceTitle: 'உங்கள் சேவையைத் தேர்ந்தெடுக்கவும்',
    selectServiceSub: 'உங்கள் வரிசை டிக்கெட்டைப் பெற வகையைத் தேர்வு செய்யவும்.',
    liveStatus: 'நேரடி நிலை',
    waitTimes: 'காத்திருப்பு நேரம்',
    waitTimesDesc: 'சராசரி காத்திருப்பு நேரம் சுமார் 8 நிமிடங்கள். உங்கள் நேரத்தை நாங்கள் மதிக்கிறோம்.',
    staffActive: '8 ஊழியர்கள் செயலில்',
    back: 'பின்செல்',
    home: 'முகப்பு',
    loading: 'ஏற்றுகிறது...',
    loadError: 'ஏற்ற முடியவில்லை. மீண்டும் முயற்சிக்கவும்.',
    homeCrumb: 'முகப்பு',
    backToMainServices: 'முதன்மை சேவைகளுக்கு திரும்பு',
    selectSubTitle: '{service} தேர்ந்தெடுக்கவும்',
    selectSubSub: 'வரிசையில் சேர குறிப்பிட்ட பரிவர்த்தனை வகையைத் தேர்வு செய்யவும்.',
    mins: 'நிமி',
    customersWaiting: 'வாடிக்கையாளர்கள் காத்திருக்கிறார்கள்',
    generatingToken: 'உங்கள் டோக்கன் உருவாக்கப்படுகிறது...',
    systemOnline: 'கணினி ஆன்லைனில்',
    branchIdLabel: 'கிளை அடையாளம்',
    digitalKiosk: 'SmartQ டிஜிட்டல் கியோஸ்க்',
    tokenReady: 'உங்கள் டோக்கன் தயார்',
    tokenReadySub: 'காத்திருப்பு பகுதிக்குச் செல்லவும். உங்கள் எண் விரைவில் அழைக்கப்படும்.',
    tokenNumber: 'டோக்கன் எண்',
    serviceLabel: 'சேவை',
    estWait: 'எஸ்ட். காத்திருப்பு',
    receiptHold: 'உங்கள் ரசீதை வைத்திருங்கள். QR குறியீடு மூலம் வரிசையைக் கண்காணிக்கலாம்.',
    printToken: 'டோக்கன் அச்சிடு',
    finish: 'முடி',
    counterPending: 'ஒதுக்கப்படவில்லை',
    scanQr: 'மொபைல் கண்காணிப்புக்கு ஸ்கேன் செய்யவும்',
    smartqBank: 'ஸ்மார்ட்க்யூ வங்கி',
    centralDistrict: 'மத்திய நிதி மாவட்டம்',
    branchNum: 'கிளை #0402',
    tokenNoLabel: 'டோக்கன் எண்',
    issued: 'வெளியிடப்பட்டது'
  }
};

const DB_MESSAGE_KEYS: Partial<Record<keyof KioskLabels, string>> = {
  selectServiceTitle: 'KIOSK_SELECT_SERVICE_TITLE',
  selectServiceSub: 'KIOSK_SELECT_SERVICE_SUBTITLE',
  tokenReady: 'TOKEN_SUCCESS_TITLE',
  tokenReadySub: 'TOKEN_SUCCESS_INSTRUCTION'
};

@Injectable({ providedIn: 'root' })
export class KioskI18nService {
  labels(code: string, messageLookup?: (key: string, fallback: string) => string): KioskLabels {
    const key = (code?.toUpperCase() ?? 'EN') as KioskLang;
    const base = { ...(LABELS[key] ?? LABELS.EN) };
    if (!messageLookup) return base;

    for (const [labelKey, messageKey] of Object.entries(DB_MESSAGE_KEYS)) {
      const k = labelKey as keyof KioskLabels;
      const fb = base[k];
      if (typeof fb === 'string' && messageKey) {
        base[k] = messageLookup(messageKey, fb) as KioskLabels[typeof k];
      }
    }
    return base;
  }

  format(text: string, params: Record<string, string>): string {
    return Object.entries(params).reduce(
      (s, [k, v]) => s.replace(new RegExp(`\\{${k}\\}`, 'g'), v),
      text
    );
  }
}
