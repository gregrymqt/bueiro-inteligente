import React from 'react';
import { Link } from 'react-router-dom';
import {
  Activity,
  ArrowRight,
  BellRing,
  Database,
  Layers3,
  MapPinned,
  Radar,
  Server,
  ShieldCheck,
  Smartphone,
  Sparkles,
} from 'lucide-react';
import styles from './About.module.scss';

interface FeatureCard {
  icon: React.ElementType;
  title: string;
  description: string;
}

interface StepCard {
  number: string;
  icon: React.ElementType;
  title: string;
  description: string;
}

interface ChannelCard {
  icon: React.ElementType;
  title: string;
  description: string;
}

const featureCards: FeatureCard[] = [
  {
    icon: Radar,
    title: 'Monitoramento contínuo',
    description: 'Leituras frequentes ajudam a enxergar mudanças no cenário antes que elas virem emergência.',
  },
  {
    icon: BellRing,
    title: 'Alertas acionáveis',
    description: 'Quando o risco aumenta, a operação recebe contexto suficiente para responder mais rápido.',
  },
  {
    icon: Database,
    title: 'Dados organizados',
    description: 'O histórico fica persistido e pronto para análise, evolução de regras e acompanhamento da malha.',
  },
  {
    icon: Smartphone,
    title: 'Acesso em múltiplas telas',
    description: 'Web e mobile compartilham a mesma visão do sistema, sem duplicar a lógica principal.',
  },
];

const stepCards: StepCard[] = [
  {
    number: '01',
    icon: MapPinned,
    title: 'Coleta em campo',
    description: 'O sensor captura a distância e envia o estado do bueiro com identificação, localização e contexto.',
  },
  {
    number: '02',
    icon: ShieldCheck,
    title: 'Validação e persistência',
    description: 'O backend valida a origem, grava no banco e atualiza o cache para reduzir latência na operação.',
  },
  {
    number: '03',
    icon: Layers3,
    title: 'Visualização em tempo real',
    description: 'As interfaces exibem o panorama do sistema e o histórico para apoiar decisões mais rápidas.',
  },
];

const channelCards: ChannelCard[] = [
  {
    icon: Activity,
    title: 'Frontend Web',
    description: 'Portal visual para acompanhar home, monitoramento e os módulos administrativos do projeto.',
  },
  {
    icon: Smartphone,
    title: 'App Android',
    description: 'Experiência móvel para consulta de dados, estado do sistema e acompanhamento em campo.',
  },
  {
    icon: Server,
    title: 'Backend ASP.NET Core',
    description: 'Camada central que valida, processa, persiste e distribui os eventos do ecossistema.',
  },
  {
    icon: Database,
    title: 'Infraestrutura de suporte',
    description: 'PostgreSQL, Redis e automações dão base para histórico, cache e tarefas recorrentes.',
  },
];

const About: React.FC = () => {
  return (
    <div className={styles.aboutPage}>
      <section className={styles.hero}>
        <div className={styles.heroContent}>
          <span className={styles.eyebrow}>
            <Sparkles size={16} />
            Tecnologia para prevenção urbana
          </span>

          <h1 className={styles.title}>Sobre nós</h1>

          <p className={styles.subtitle}>
            O Bueiro Inteligente conecta sensores, backend, web, mobile e infraestrutura em uma cadeia simples de operar,
            pensada para antecipar riscos de obstrução e apoiar decisões antes que o problema chegue à rua.
          </p>

          <div className={styles.actions}>
            <Link className={styles.primaryAction} to="/">
              Explorar a Home
              <ArrowRight size={18} />
            </Link>

            <Link className={styles.secondaryAction} to="/login">
              Entrar no sistema
            </Link>
          </div>

          <ul className={styles.heroFacts} aria-label="Destaques do projeto">
            <li className={styles.heroFact}>
              <MapPinned size={16} />
              <span>Leitura de campo com contexto geográfico</span>
            </li>
            <li className={styles.heroFact}>
              <ShieldCheck size={16} />
              <span>Fluxo pensado para validação e confiabilidade</span>
            </li>
            <li className={styles.heroFact}>
              <BellRing size={16} />
              <span>Operação orientada a resposta rápida</span>
            </li>
          </ul>
        </div>

        <div className={styles.heroVisual} aria-hidden="true">
          <article className={styles.visualCard}>
            <span className={styles.visualTag}>Fluxo em tempo real</span>
            <h2>Sensor → API → Painéis</h2>
            <p>Uma arquitetura distribuída para levar o dado do campo até a tela com clareza e velocidade.</p>

            <div className={styles.visualStats}>
              <div className={styles.visualStat}>
                <span>Coleta</span>
                <strong>ESP32 + sensores</strong>
              </div>
              <div className={styles.visualStat}>
                <span>Cache</span>
                <strong>Redis para resposta rápida</strong>
              </div>
              <div className={styles.visualStat}>
                <span>Alertas</span>
                <strong>Web e mobile sincronizados</strong>
              </div>
            </div>
          </article>

          <div className={styles.visualStack}>
            <article className={styles.visualMiniCard}>
              <Radar size={20} />
              <div>
                <strong>Leitura contínua</strong>
                <span>Estado atualizado em ciclos curtos.</span>
              </div>
            </article>

            <article className={styles.visualMiniCardAlt}>
              <Database size={20} />
              <div>
                <strong>Histórico confiável</strong>
                <span>Dados persistidos para análise e operação.</span>
              </div>
            </article>
          </div>
        </div>
      </section>

      <section className={styles.section}>
        <div className={styles.sectionHeader}>
          <span className={styles.sectionKicker}>O que o sistema entrega</span>
          <h2>Prevenção com leitura clara e operação rápida</h2>
          <p>
            O objetivo é traduzir leituras técnicas em uma experiência fácil de entender, para que a equipe saiba o que está
            acontecendo e o que fazer em seguida.
          </p>
        </div>

        <div className={styles.highlightsGrid}>
          {featureCards.map((card) => {
            const CardIcon = card.icon;

            return (
              <article className={styles.highlightCard} key={card.title}>
                <span className={styles.highlightIcon}>
                  <CardIcon size={22} />
                </span>
                <h3>{card.title}</h3>
                <p>{card.description}</p>
              </article>
            );
          })}
        </div>
      </section>

      <section className={styles.section}>
        <div className={styles.sectionHeader}>
          <span className={styles.sectionKicker}>Como funciona</span>
          <h2>Uma trilha simples do sensor até a decisão</h2>
          <p>
            A experiência foi pensada para reduzir fricção: coleta em campo, processamento centralizado e visualização pronta
            para apoiar a operação.
          </p>
        </div>

        <div className={styles.timeline}>
          {stepCards.map((step) => {
            const StepIcon = step.icon;

            return (
              <article className={styles.stepCard} key={step.number}>
                <span className={styles.stepNumber}>{step.number}</span>
                <span className={styles.stepIcon}>
                  <StepIcon size={20} />
                </span>
                <h3>{step.title}</h3>
                <p>{step.description}</p>
              </article>
            );
          })}
        </div>
      </section>

      <section className={styles.section}>
        <div className={styles.sectionHeader}>
          <span className={styles.sectionKicker}>Camadas integradas</span>
          <h2>Web, mobile, backend e infraestrutura na mesma linguagem</h2>
          <p>
            Cada camada cumpre um papel específico, mas todas conversam pelo mesmo contrato para manter a experiência
            consistente.
          </p>
        </div>

        <div className={styles.channelsGrid}>
          {channelCards.map((channel) => {
            const ChannelIcon = channel.icon;

            return (
              <article className={styles.channelCard} key={channel.title}>
                <span className={styles.channelIcon}>
                  <ChannelIcon size={20} />
                </span>
                <h3>{channel.title}</h3>
                <p>{channel.description}</p>
              </article>
            );
          })}
        </div>
      </section>

      <section className={styles.cta}>
        <div className={styles.ctaContent}>
          <span className={styles.ctaKicker}>Pronto para explorar</span>
          <h2>Conheça o ecossistema ou acesse a operação</h2>
          <p>
            Se você quer entender o projeto, a Home resume o panorama geral. Se já tem acesso, entre e siga para o
            monitoramento.
          </p>
        </div>

        <div className={styles.ctaActions}>
          <Link to="/" className={styles.primaryAction}>
            Voltar para a Home
          </Link>
          <Link to="/login" className={styles.secondaryAction}>
            Entrar
          </Link>
        </div>
      </section>
    </div>
  );
};

export default About;