export const languages = {
  ja: '日本語',
  en: 'English',
} as const;

export const defaultLang = 'ja' as const;

export const ui = {
  ja: {
    // Navigation
    'nav.features': '特徴',
    'nav.pricing': '料金',
    'nav.howto': '使い方',
    'nav.download': 'ダウンロード',
    'nav.faq': 'FAQ',

    // Hero
    'hero.badge': 'Windows 10/11',
    'hero.title': '声で、入力する。',
    'hero.subtitle': 'ホットキーを押すだけで音声をテキストに変換。OpenAI APIで高精度な文字起こし。',
    'hero.cta': 'ダウンロード',
    'hero.cta.github': 'GitHub',

    // Features
    'features.title': '特徴',
    'features.subtitle': 'シンプルで強力な音声入力',
    'features.push-to-talk.title': 'Push-to-Talk',
    'features.push-to-talk.desc': 'ホットキーを押している間だけ録音。シンプルで直感的な操作。',
    'features.openai.title': 'OpenAI API連携',
    'features.openai.desc': 'gpt-4o-mini-transcribe または whisper-1 による高精度な音声認識。',
    'features.auto-paste.title': '自動ペースト',
    'features.auto-paste.desc': '変換されたテキストは自動的にアクティブなアプリに貼り付け。',
    'features.secure.title': 'セキュア',
    'features.secure.desc': 'APIキーはWindows DPAPIで暗号化して安全に保存。',
    'features.tray.title': 'システムトレイ常駐',
    'features.tray.desc': 'バックグラウンドで静かに動作。いつでもすぐに使える。',
    'features.standalone.title': 'ランタイム不要',
    'features.standalone.desc': '単一EXEファイルで配布。.NETランタイムのインストール不要。',

    // Pricing
    'pricing.title': 'サブスクではなく、従量課金',
    'pricing.subtitle': '使った分だけ支払う、シンプルな料金体系',
    'pricing.desc': 'oto は OpenAI API を使用。月額固定ではなく、使った分だけ支払い。',
    'pricing.example.title': '料金の目安',
    'pricing.example.rate': '約 $0.006 / 分',
    'pricing.example.daily': '1日10分使用の場合',
    'pricing.example.monthly': '月額約 $1.8',
    'pricing.example.note': '※ OpenAI gpt-4o-mini-transcribe の場合',
    'pricing.compare.title': '他のサービスとの比較',
    'pricing.compare.service': 'サービス',
    'pricing.compare.price': '料金',
    'pricing.compare.type': 'タイプ',
    'pricing.compare.oto': 'oto + OpenAI API',
    'pricing.compare.oto.price': '約 $1.8/月',
    'pricing.compare.oto.type': '従量課金',
    'pricing.compare.other1': 'Otter.ai',
    'pricing.compare.other1.price': '$16.99/月',
    'pricing.compare.other1.type': 'サブスク',
    'pricing.compare.other2': 'その他SaaS',
    'pricing.compare.other2.price': '$10-20/月',
    'pricing.compare.other2.type': 'サブスク',

    // How it works
    'howto.title': '使い方',
    'howto.subtitle': '4つのステップで簡単セットアップ',
    'howto.step1.title': 'ダウンロード',
    'howto.step1.desc': 'GitHubからEXEファイルをダウンロード',
    'howto.step2.title': 'APIキー設定',
    'howto.step2.desc': 'OpenAI APIキーを設定画面で入力',
    'howto.step3.title': 'ホットキーを押す',
    'howto.step3.desc': 'Ctrl+Alt+Spaceを押しながら話す',
    'howto.step4.title': '自動入力',
    'howto.step4.desc': 'テキストが自動的にペーストされる',

    // Download
    'download.title': 'ダウンロード',
    'download.subtitle': 'Windows 10/11 対応',
    'download.cta': '最新版をダウンロード',
    'download.requirements': 'システム要件',
    'download.req.os': 'Windows 10/11 (64ビット)',
    'download.req.api': 'OpenAI APIキー',
    'download.req.mic': 'マイク',

    // FAQ
    'faq.title': 'よくある質問',
    'faq.q1': 'OpenAI APIキーはどこで取得できますか？',
    'faq.a1': 'OpenAIの公式サイト (platform.openai.com) でアカウントを作成し、APIキーを発行できます。',
    'faq.q2': 'APIの料金はどのくらいかかりますか？',
    'faq.a2': 'gpt-4o-mini-transcribe の場合、1分あたり約$0.006です。1日10分使っても月額約$1.8程度です。',
    'faq.q3': 'ホットキーは変更できますか？',
    'faq.a3': 'はい、設定画面からお好みのキーの組み合わせに変更できます。',
    'faq.q4': 'インターネット接続は必要ですか？',
    'faq.a4': 'はい、OpenAI APIを使用するためインターネット接続が必要です。',
    'faq.q5': 'どの言語に対応していますか？',
    'faq.a5': 'OpenAIのWhisperモデルは多数の言語に対応しています。日本語、英語はもちろん、その他多くの言語で利用可能です。',

    // Footer
    'footer.copyright': 'oto - 音声入力アプリ',
    'footer.github': 'GitHub',
    'footer.license': 'MIT License',
  },
  en: {
    // Navigation
    'nav.features': 'Features',
    'nav.pricing': 'Pricing',
    'nav.howto': 'How to Use',
    'nav.download': 'Download',
    'nav.faq': 'FAQ',

    // Hero
    'hero.badge': 'Windows 10/11',
    'hero.title': 'Voice to Text, Instantly.',
    'hero.subtitle': 'Press a hotkey to transcribe your voice. Powered by OpenAI for accurate transcription.',
    'hero.cta': 'Download',
    'hero.cta.github': 'GitHub',

    // Features
    'features.title': 'Features',
    'features.subtitle': 'Simple yet powerful voice input',
    'features.push-to-talk.title': 'Push-to-Talk',
    'features.push-to-talk.desc': 'Record only while holding the hotkey. Simple and intuitive.',
    'features.openai.title': 'OpenAI Integration',
    'features.openai.desc': 'Accurate speech recognition with gpt-4o-mini-transcribe or whisper-1.',
    'features.auto-paste.title': 'Auto Paste',
    'features.auto-paste.desc': 'Transcribed text is automatically pasted into the active application.',
    'features.secure.title': 'Secure',
    'features.secure.desc': 'API keys are encrypted with Windows DPAPI.',
    'features.tray.title': 'System Tray',
    'features.tray.desc': 'Runs quietly in the background. Always ready when you need it.',
    'features.standalone.title': 'No Runtime Required',
    'features.standalone.desc': 'Single EXE distribution. No .NET runtime installation needed.',

    // Pricing
    'pricing.title': 'Pay As You Go, No Subscription',
    'pricing.subtitle': 'Simple pricing - pay only for what you use',
    'pricing.desc': 'oto uses OpenAI API. Pay only for what you use, not a monthly fee.',
    'pricing.example.title': 'Pricing Example',
    'pricing.example.rate': '~$0.006 / min',
    'pricing.example.daily': 'Using 10 min/day',
    'pricing.example.monthly': '~$1.8 / month',
    'pricing.example.note': '* Based on OpenAI gpt-4o-mini-transcribe',
    'pricing.compare.title': 'Comparison with Other Services',
    'pricing.compare.service': 'Service',
    'pricing.compare.price': 'Price',
    'pricing.compare.type': 'Type',
    'pricing.compare.oto': 'oto + OpenAI API',
    'pricing.compare.oto.price': '~$1.8/mo',
    'pricing.compare.oto.type': 'Pay-as-you-go',
    'pricing.compare.other1': 'Otter.ai',
    'pricing.compare.other1.price': '$16.99/mo',
    'pricing.compare.other1.type': 'Subscription',
    'pricing.compare.other2': 'Other SaaS',
    'pricing.compare.other2.price': '$10-20/mo',
    'pricing.compare.other2.type': 'Subscription',

    // How it works
    'howto.title': 'How to Use',
    'howto.subtitle': 'Easy setup in 4 steps',
    'howto.step1.title': 'Download',
    'howto.step1.desc': 'Download the EXE file from GitHub',
    'howto.step2.title': 'Set API Key',
    'howto.step2.desc': 'Enter your OpenAI API key in settings',
    'howto.step3.title': 'Press Hotkey',
    'howto.step3.desc': 'Hold Ctrl+Alt+Space and speak',
    'howto.step4.title': 'Auto Input',
    'howto.step4.desc': 'Text is automatically pasted',

    // Download
    'download.title': 'Download',
    'download.subtitle': 'For Windows 10/11',
    'download.cta': 'Download Latest Version',
    'download.requirements': 'System Requirements',
    'download.req.os': 'Windows 10/11 (64-bit)',
    'download.req.api': 'OpenAI API Key',
    'download.req.mic': 'Microphone',

    // FAQ
    'faq.title': 'Frequently Asked Questions',
    'faq.q1': 'Where can I get an OpenAI API key?',
    'faq.a1': 'Create an account on OpenAI\'s official site (platform.openai.com) and generate an API key.',
    'faq.q2': 'How much does the API cost?',
    'faq.a2': 'With gpt-4o-mini-transcribe, it\'s about $0.006 per minute. Using 10 minutes daily costs around $1.8/month.',
    'faq.q3': 'Can I change the hotkey?',
    'faq.a3': 'Yes, you can customize the key combination in the settings.',
    'faq.q4': 'Is an internet connection required?',
    'faq.a4': 'Yes, an internet connection is required to use the OpenAI API.',
    'faq.q5': 'What languages are supported?',
    'faq.a5': 'OpenAI\'s Whisper model supports many languages including Japanese, English, and many others.',

    // Footer
    'footer.copyright': 'oto - Voice Input App',
    'footer.github': 'GitHub',
    'footer.license': 'MIT License',
  }
} as const;

export type UIKey = keyof typeof ui.ja;
