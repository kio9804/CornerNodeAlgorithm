import { NestFactory } from '@nestjs/core';
import { NestExpressApplication } from '@nestjs/platform-express';
import * as path from 'path';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create<NestExpressApplication>(AppModule);

  app.useStaticAssets(path.join(__dirname, './common', 'uploads'), {
    prefix: '/maps',
  });
  const port = process.env.PORT || 3000;
  await app.listen(port);
}
bootstrap();
